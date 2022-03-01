using Autofac;
using Cysharp.Threading.Tasks;
using Deathmatch.API.Matches;
using Deathmatch.API.Matches.Events;
using Deathmatch.API.Matches.Registrations;
using Deathmatch.API.Players;
using Deathmatch.Core.Helpers;
using Deathmatch.Core.Matches.Events;
using Deathmatch.Core.Matches.Registrations;
using Deathmatch.Core.Players.Events;
using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;
using OpenMod.API;
using OpenMod.API.Commands;
using OpenMod.API.Eventing;
using OpenMod.API.Ioc;
using OpenMod.API.Plugins;
using OpenMod.API.Prioritization;
using OpenMod.Core.Ioc;
using SilK.Unturned.Extras.Events;
using SilK.Unturned.Extras.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Deathmatch.Core.Matches
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Lowest)]
    public class MatchExecutor : IMatchExecutor,
        IInstanceEventListener<IMatchEndedEvent>
    {
        public IMatch? CurrentMatch { get; private set; }

        private readonly IRuntime _runtime;
        private readonly IEventBus _eventBus;
        private readonly IMatchManager _matchManager;
        private readonly IPluginActivator _pluginActivator;
        private readonly IStringLocalizerAccessor<DeathmatchPlugin> _stringLocalizer;
        
        private readonly List<IGamePlayer> _participants;
        private readonly AsyncLock _matchLock;

        public MatchExecutor(IRuntime runtime,
            IEventBus eventBus,
            IMatchManager matchManager,
            IPluginActivator pluginActivator,
            IStringLocalizerAccessor<DeathmatchPlugin> stringLocalizer)
        {
            _runtime = runtime;
            _eventBus = eventBus;
            _matchManager = matchManager;
            _pluginActivator = pluginActivator;
            _stringLocalizer = stringLocalizer;
            
            _participants = new List<IGamePlayer>();
            _matchLock = new AsyncLock();
        }

        public IReadOnlyCollection<IGamePlayer> GetParticipants() => _participants.AsReadOnly();

        public async UniTask<bool> AddParticipant(IGamePlayer player)
        {
            if (_participants.Contains(player))
            {
                return false;
            }

            await UniTask.SwitchToMainThread();

            player.ClearMatchData();

            _participants.Add(player);

            if (CurrentMatch == null || CurrentMatch.Players.Contains(player))
            {
                return true;
            }

            var joiningEvent = new GamePlayerJoiningMatchEvent(player, CurrentMatch);
            await EmitEvent(joiningEvent);

            if (joiningEvent.IsCancelled)
            {
                return false;
            }

            await CurrentMatch.AddPlayer(player);
            player.CurrentMatch = CurrentMatch;

            await EmitEvent(new GamePlayerJoinedMatchEvent(player, CurrentMatch));

            return true;
        }

        public async UniTask RemoveParticipant(IGamePlayer player)
        {
            if (_participants.Contains(player))
            {
                await UniTask.SwitchToMainThread();

                player.ClearMatchData();

                _participants.Remove(player);

                if (CurrentMatch != null && CurrentMatch.Players.Contains(player))
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

        private void CheckMatchInstance()
        {
            if (CurrentMatch == null)
            {
                return;
            }

            if (CurrentMatch.Status == MatchStatus.Initialized || CurrentMatch.Status == MatchStatus.Ended ||
                CurrentMatch.Status == MatchStatus.ExceptionWhenEnding)
            {
                CurrentMatch = null;
            }
        }

        private IMatchRegistration GetRandomMatchRegistration()
        {
            var registrations = _matchManager.GetEnabledMatchRegistrations();

            if (registrations.Count == 0)
            {
                throw new UserFriendlyException(_stringLocalizer.GetInstance()["errors:no_registrations"]);
            }

            return registrations.RandomElement();
        }

        private ILifetimeScope GetScopeFromRegistration(IMatchRegistration registration)
        {
            var plugins = _pluginActivator.ActivatedPlugins;

            var plugin = plugins.FirstOrDefault(x => x.GetType().Assembly == registration.Type.Assembly);

            return plugin?.LifetimeScope ?? throw new Exception("Could not get match game mode's plugin instance");
        }

        private void BuildMatchScope(ContainerBuilder builder, IMatchRegistration registration)
        {
            builder.Register(_ => new MatchRegistrationAccessor(registration))
                .AsSelf()
                .As<IMatchRegistrationAccessor>();

            builder.RegisterType(registration.Type)
                .AsSelf()
                .As<IMatch>()
                .SingleInstance()
                .OwnedByLifetimeScope();
        }

        private IMatch CreateMatchInstance(IMatchRegistration registration)
        {
            // Use the plugin's lifetime scope
            var scope = GetScopeFromRegistration(registration);

            // Create child scope
            var matchScope = scope.BeginLifetimeScopeEx(builder => BuildMatchScope(builder, registration));

            return (IMatch?)matchScope.Resolve(registration.Type) ??
                   throw new Exception($"Unable to create instance of {registration.Type}.");
        }

        private async Task EmitEvent(IEvent @event)
        {
            await _eventBus.EmitAsync(_runtime, this, @event);
        }

        public async UniTask<bool> StartMatch(IMatchRegistration? registration = null)
        {
            await UniTask.SwitchToThreadPool();

            using var matchLock = await _matchLock.LockAsync();

            CheckMatchInstance();

            if (CurrentMatch != null)
            {
                return false;
            }

            try
            {
                registration ??= GetRandomMatchRegistration();

                CurrentMatch = CreateMatchInstance(registration);

                // Emit MatchStartingEvent 
                var startingEvent = new MatchStartingEvent(CurrentMatch);
                await EmitEvent(startingEvent);

                // If start cancelled
                if (startingEvent.IsCancelled)
                {
                    throw new UserFriendlyException(_stringLocalizer["errors:match_start_cancelled"]);
                }

                // Start match
                await CurrentMatch.StartAsync(_participants);

                // Emit MatchStartedEvent
                await EmitEvent(new MatchStartedEvent(CurrentMatch));

                return true;
            }
            catch
            {
                CurrentMatch = null;
                throw;
            }
        }

        public UniTask HandleEventAsync(object? sender, IMatchEndedEvent @event)
        {
            if (@event.Match == CurrentMatch)
            {
                CurrentMatch = null;
            }

            return UniTask.CompletedTask;
        }
    }
}
