using Autofac;
using Cysharp.Threading.Tasks;
using Deathmatch.API.Matches;
using Deathmatch.API.Matches.Events;
using Deathmatch.API.Matches.Registrations;
using Deathmatch.API.Players;
using Deathmatch.Core.Helpers;
using Deathmatch.Core.Matches.Events;
using Deathmatch.Core.Matches.Extensions;
using Deathmatch.Core.Matches.Registrations;
using Deathmatch.Core.Players.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using OpenMod.API;
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
        private readonly ILogger<MatchExecutor> _logger;
        private readonly ILifetimeScope _lifetimeScope;
        
        private readonly List<IGamePlayer> _participants;
        private readonly AsyncLock _matchLock;

        public MatchExecutor(IRuntime runtime,
            IEventBus eventBus,
            IMatchManager matchManager,
            IPluginActivator pluginActivator,
            IStringLocalizerAccessor<DeathmatchPlugin> stringLocalizer,
            ILogger<MatchExecutor> logger,
            ILifetimeScope lifetimeScope)
        {
            _runtime = runtime;
            _eventBus = eventBus;
            _matchManager = matchManager;
            _pluginActivator = pluginActivator;
            _stringLocalizer = stringLocalizer;
            _logger = logger;
            _lifetimeScope = lifetimeScope;
            
            _participants = new List<IGamePlayer>();
            _matchLock = new AsyncLock();
        }

        public IReadOnlyCollection<IGamePlayer> GetParticipants() => _participants.AsReadOnly();

        public async UniTask AddParticipant(IGamePlayer player)
        {
            if (!_participants.Contains(player))
            {
                await UniTask.SwitchToMainThread();

                player.ClearMatchData();

                _participants.Add(player);

                if (CurrentMatch != null && !CurrentMatch.Players.Contains(player))
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
                    await player.PrintMessageAsync(_stringLocalizer.GetInstance()["commands:join:success"]);
                }
            }
            else
            {
                await player.PrintMessageAsync(_stringLocalizer.GetInstance()["commands:join:already"]);
            }
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
                    await player.PrintMessageAsync(_stringLocalizer.GetInstance()["commands:leave:success"]);
                }
            }
            else
            {
                await player.PrintMessageAsync(_stringLocalizer.GetInstance()["commands:leave:already"]);
            }
        }

        public async UniTask<bool> StartMatch(IMatchRegistration? registration = null)
        {
            await UniTask.SwitchToThreadPool();

            using (await _matchLock.LockAsync())
            {
                if (CurrentMatch != null && CurrentMatch.Status != MatchStatus.Initialized)
                {
                    return false;
                }

                await UniTask.SwitchToThreadPool();

                IMatch? match = null;

                try
                {
                    if (registration == null)
                    {
                        var registrations = _matchManager.GetEnabledMatchRegistrations();

                        if (registrations.Count == 0)
                        {
                            return false;
                        }

                        registration = registrations.RandomElement();
                    }

                    var scope = _lifetimeScope;

                    // Use the plugin's lifetime scope
                    var plugin = _pluginActivator.ActivatedPlugins.FirstOrDefault(x =>
                        x.GetType().Assembly == registration.Type.Assembly);

                    if (plugin != null)
                    {
                        scope = plugin.LifetimeScope;
                    }

                    // Create scope

                    var matchScope = scope.BeginLifetimeScopeEx(builder =>
                    {
                        builder.Register(_ => new MatchRegistrationAccessor(registration))
                            .AsSelf()
                            .As<IMatchRegistrationAccessor>();

                        builder.RegisterType(registration.Type)
                            .AsSelf()
                            .As<IMatch>()
                            .SingleInstance()
                            .OwnedByLifetimeScope();
                    });

                    match = (IMatch?)matchScope.Resolve(registration.Type);
                    
                    if (match == null)
                    {
                        throw new Exception($"Unable to create instance of {registration.Type.Name}.");
                    }

                    // Emit MatchStartingEvent 

                    var startingEvent = new MatchStartingEvent(match);

                    await _eventBus.EmitAsync(_runtime, this, startingEvent);

                    if (startingEvent.IsCancelled)
                    {
                        return false;
                    }

                    // Start match

                    CurrentMatch = match;

                    await CurrentMatch.StartAsync(_participants);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception occurred when attempting to start match");

                    return false;
                }

                CurrentMatch = match;

                return true;
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
