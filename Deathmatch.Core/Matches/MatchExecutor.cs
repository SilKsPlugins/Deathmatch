using Autofac;
using Cysharp.Threading.Tasks;
using Deathmatch.API.Matches;
using Deathmatch.API.Players;
using Deathmatch.Core.Matches.Events;
using Deathmatch.Core.Players.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OpenMod.API;
using OpenMod.API.Eventing;
using OpenMod.API.Ioc;
using OpenMod.API.Plugins;
using OpenMod.API.Prioritization;
using OpenMod.Core.Localization;
using OpenMod.Core.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Deathmatch.Core.Matches
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Lowest)]
    public class MatchExecutor : IMatchExecutor
    {
        private readonly IRuntime _runtime;
        private readonly IEventBus _eventBus;
        private readonly IMatchManager _matchManager;
        private readonly IStringLocalizer _stringLocalizer;
        private readonly IPluginAccessor<DeathmatchPlugin> _pluginAccessor;
        private readonly IPluginActivator _pluginActivator;
        private readonly IServiceProvider _serviceProvider;


        private readonly Random _rng;
        private readonly List<IGamePlayer> _participants;
        private readonly HashSet<Type> _subscribedTotalEvents;

        public MatchExecutor(IRuntime runtime,
            IEventBus eventBus,
            IMatchManager matchManager,
            IPluginAccessor<DeathmatchPlugin> pluginAccessor,
            IPluginActivator pluginActivator,
            IStringLocalizerFactory stringLocalizerFactory,
            IServiceProvider serviceProvider)
        {
            _runtime = runtime;
            _eventBus = eventBus;
            _matchManager = matchManager;
            _pluginAccessor = pluginAccessor;
            _pluginActivator = pluginActivator;
            _serviceProvider = serviceProvider;

            string workingDir = PluginHelper.GetWorkingDirectory(_runtime, "Deathmatch.Core");

            _stringLocalizer ??= Directory.Exists(workingDir)
                ? _stringLocalizer = stringLocalizerFactory.Create("translations", workingDir)
                : NullStringLocalizer.Instance;

            _rng = new Random();
            _participants = new List<IGamePlayer>();
            _subscribedTotalEvents = new HashSet<Type>();
        }

        public IReadOnlyCollection<IGamePlayer> GetParticipants() => _participants.AsReadOnly();

        public async UniTask AddParticipant(IGamePlayer player)
        {
            if (!_participants.Contains(player))
            {
                await UniTask.SwitchToMainThread();

                player.ClearMatchData();

                _participants.Add(player);

                if (CurrentMatch != null && !CurrentMatch.GetPlayers().Contains(player))
                {
                    var preEvent = new GamePlayerJoiningMatchEvent(player, CurrentMatch);
                    await _eventBus.EmitAsync(_runtime, this, preEvent);
                    if (preEvent.IsCancelled) return;

                    await CurrentMatch.AddPlayer(player);

                    player.CurrentMatch = CurrentMatch;

                    var postEvent = new GamePlayerJoinedMatchEvent(player, CurrentMatch);
                    await _eventBus.EmitAsync(_runtime, this, postEvent);
                }
                else
                {
                    await player.PrintMessageAsync(_stringLocalizer["commands:join:success"]);
                }
            }
            else
            {
                await player.PrintMessageAsync(_stringLocalizer["commands:join:already"]);
            }
        }

        public async UniTask RemoveParticipant(IGamePlayer player)
        {
            if (_participants.Contains(player))
            {
                await UniTask.SwitchToMainThread();

                player.ClearMatchData();

                _participants.Remove(player);

                if (CurrentMatch != null && CurrentMatch.GetPlayers().Contains(player))
                {
                    var preEvent = new GamePlayerLeavingMatchEvent(player, CurrentMatch);
                    await _eventBus.EmitAsync(_runtime, this, preEvent);
                    if (preEvent.IsCancelled) return;

                    await CurrentMatch.RemovePlayer(player);

                    player.CurrentMatch = null;

                    var postEvent = new GamePlayerLeftMatchEvent(player, CurrentMatch);
                    await _eventBus.EmitAsync(_runtime, this, postEvent);
                }
                else
                {
                    await player.PrintMessageAsync(_stringLocalizer["commands:leave:success"]);
                }
            }
            else
            {
                await player.PrintMessageAsync(_stringLocalizer["commands:leave:already"]);
            }
        }

        public IMatch CurrentMatch { get; private set; }

        private Dictionary<Type, MethodInfo> _subscribedMatchEvents;

        public async UniTask<bool> StartMatch(IMatchRegistration registration = null)
        {
            if (CurrentMatch != null && CurrentMatch.IsRunning)
            {
                return false;
            }

            if (registration == null)
            {
                var registrations = _matchManager.GetEnabledMatchRegistrations();

                if (registrations == null || registrations.Count == 0) return false;

                registration = registrations.ElementAt(_rng.Next(registrations.Count));
            }

            var serviceProvider = _serviceProvider;

            // Use the plugin's service provider
            if (registration is RegisteredMatch match)
            {
                var plugin = _pluginActivator.ActivatedPlugins.FirstOrDefault(x =>
                    x.GetType().Assembly == match.MatchType.Assembly);

                if (plugin != null)
                {
                    serviceProvider = plugin.LifetimeScope.Resolve<IServiceProvider>();
                }
            }

            CurrentMatch = registration.Instantiate(serviceProvider);

            if (CurrentMatch == null)
            {
                throw new Exception($"Unable to create instance of {nameof(IMatch)} with id {registration.Id}");
            }

            CurrentMatch.Registration = registration;

            // Custom event implementation

            var eventListeners = CurrentMatch.GetType().GetInterfaces().Where(x =>
                x.IsGenericType && x.GetGenericTypeDefinition().IsAssignableFrom(typeof(IMatchEventListener<>)));

            _subscribedMatchEvents = new Dictionary<Type, MethodInfo>();

            foreach (var listener in eventListeners)
            {
                var eventType = listener.GetGenericArguments().Single();

                _subscribedMatchEvents.Add(eventType, listener.GetMethod("HandleEventAsync", BindingFlags.Public | BindingFlags.Instance));

                if (!_subscribedTotalEvents.Contains(eventType))
                {
                    _subscribedTotalEvents.Add(eventType);

                    _eventBus.Subscribe(_pluginAccessor.Instance, eventType, HandleEventAsync);
                }
            }

            foreach (IGamePlayer player in _participants)
            {
                player.ClearMatchData();

                player.CurrentMatch = CurrentMatch;

                await CurrentMatch.AddPlayer(player);
            }

            var startingEvent = new MatchStartingEvent(CurrentMatch);

            await _eventBus.EmitAsync(_runtime, this, startingEvent);

            if (startingEvent.IsCancelled)
            {
                return false;
            }

            bool success = await CurrentMatch.StartMatch();

            if (success)
            {
                var startedEvent = new MatchStartedEvent(CurrentMatch);

                await _eventBus.EmitAsync(_runtime, this, startedEvent);
            }
            else
            {
                CurrentMatch = null;
            }

            return success;
        }

        public async UniTask<bool> EndMatch()
        {
            if (CurrentMatch == null || !CurrentMatch.IsRunning)
            {
                return false;
            }

            await UniTask.SwitchToMainThread();

            bool success = await CurrentMatch.EndMatch();

            foreach (var player in _participants)
            {
                player.ClearMatchData();
                player.CurrentMatch = null;
            }

            if (success)
            {
                var endedEvent = new MatchEndedEvent(CurrentMatch);

                await _eventBus.EmitAsync(_runtime, this, endedEvent);
            }

            CurrentMatch = null;

            return success;
        }

        public async Task HandleEventAsync(IServiceProvider serviceProvider, object sender, IEvent @event)
        {
            if (CurrentMatch == null || !CurrentMatch.IsRunning) return;

            if (_subscribedMatchEvents.TryGetValue(@event.GetType(), out var method))
            {
                await (Task)method.Invoke(CurrentMatch, new[] { sender, @event });
            }
        }
    }
}
