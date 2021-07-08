using Autofac;
using Cysharp.Threading.Tasks;
using Deathmatch.API.Matches;
using Deathmatch.API.Players;
using Deathmatch.Core.Matches.Events;
using Deathmatch.Core.Players.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OpenMod.API;
using OpenMod.API.Eventing;
using OpenMod.API.Ioc;
using OpenMod.API.Plugins;
using OpenMod.API.Prioritization;
using OpenMod.Core.Localization;
using OpenMod.Core.Plugins;
using SilK.Unturned.Extras.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Deathmatch.Core.Matches
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Lowest)]
    public class MatchExecutor : IMatchExecutor
    {
        public IMatch? CurrentMatch { get; private set; }

        private readonly IRuntime _runtime;
        private readonly IEventBus _eventBus;
        private readonly IMatchManager _matchManager;
        private readonly IStringLocalizer _stringLocalizer;
        private readonly IPluginAccessor<DeathmatchPlugin> _pluginAccessor;
        private readonly IPluginActivator _pluginActivator;
        private readonly IEventSubscriber _eventSubscriber;
        private readonly ILogger<MatchExecutor> _logger;
        private readonly IServiceProvider _serviceProvider;

        private readonly Random _rng;
        private readonly List<IGamePlayer> _participants;
        private IDisposable? _matchEventSubscriptionDisposer;

        public MatchExecutor(IRuntime runtime,
            IEventBus eventBus,
            IMatchManager matchManager,
            IPluginAccessor<DeathmatchPlugin> pluginAccessor,
            IPluginActivator pluginActivator,
            IStringLocalizerFactory stringLocalizerFactory,
            IEventSubscriber eventSubscriber,
            ILogger<MatchExecutor> logger,
            IServiceProvider serviceProvider)
        {
            _runtime = runtime;
            _eventBus = eventBus;
            _matchManager = matchManager;
            _pluginAccessor = pluginAccessor;
            _pluginActivator = pluginActivator;
            _eventSubscriber = eventSubscriber;
            _logger = logger;
            _serviceProvider = serviceProvider;

            string workingDir = PluginHelper.GetWorkingDirectory(_runtime, "Deathmatch.Core");

            _stringLocalizer ??= Directory.Exists(workingDir)
                ? _stringLocalizer = stringLocalizerFactory.Create("translations", workingDir)
                : NullStringLocalizer.Instance;

            _rng = new Random();
            _participants = new List<IGamePlayer>();
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

        public async UniTask<bool> StartMatch(IMatchRegistration? registration = null)
        {
            if (CurrentMatch != null && CurrentMatch.IsRunning)
            {
                return false;
            }

            if (registration == null)
            {
                var registrations = _matchManager.GetEnabledMatchRegistrations();

                if (registrations.Count == 0)
                {
                    return false;
                }

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

            // Create instance

            CurrentMatch = registration.Instantiate(serviceProvider);

            if (CurrentMatch == null)
            {
                throw new Exception($"Unable to create instance of {nameof(IMatch)} with id {registration.Id}");
            }

            CurrentMatch.Registration = registration;

            // Subscribe events

            _matchEventSubscriptionDisposer = _eventSubscriber.Subscribe(CurrentMatch, _runtime);

            // Emit MatchStartingEvent 

            var startingEvent = new MatchStartingEvent(CurrentMatch);

            await _eventBus.EmitAsync(_runtime, this, startingEvent);

            if (startingEvent.IsCancelled)
            {
                return false;
            }
            
            // Prepare players

            foreach (IGamePlayer player in _participants)
            {
                player.ClearMatchData();

                player.CurrentMatch = CurrentMatch;

                await CurrentMatch.AddPlayer(player);
            }

            // Start match

            var success = false;

            try
            {
                success = await CurrentMatch.StartMatch();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred when attempting to start match");
            }

            if (success)
            {
                // Emit MatchStartedEvent

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

            var success = false;

            try
            {
                success = await CurrentMatch.EndMatch();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred when attempting to end match");
            }

            if (!success)
            {
                return false;
            }

            // Clean up players

            foreach (var player in _participants)
            {
                player.ClearMatchData();
                player.CurrentMatch = null;
            }

            // Unsubscribe events

            _matchEventSubscriptionDisposer?.Dispose();

            // Emit MatchEndedEvent

            var endedEvent = new MatchEndedEvent(CurrentMatch);

            await _eventBus.EmitAsync(_runtime, this, endedEvent);

            // Clear current match

            CurrentMatch = null;

            return true;
        }
    }
}
