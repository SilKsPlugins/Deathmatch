using Autofac;
using Cysharp.Threading.Tasks;
using Deathmatch.API.Loadouts;
using Deathmatch.API.Matches;
using Deathmatch.API.Matches.Registrations;
using Deathmatch.API.Players;
using Deathmatch.API.Preservation;
using Deathmatch.Core.Grace;
using Deathmatch.Core.Loadouts;
using Deathmatch.Core.Matches.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using OpenMod.API;
using OpenMod.API.Eventing;
using OpenMod.API.Permissions;
using OpenMod.API.Users;
using SilK.Unturned.Extras.Configuration;
using SilK.Unturned.Extras.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Deathmatch.Core.Matches
{
    public abstract class MatchBase<TConfig, TLoadoutCategory> : IMatch, IAsyncDisposable
        where TConfig : class
        where TLoadoutCategory : class, ILoadoutCategory
    {
        protected Random Rng = new();

        protected readonly IOpenModComponent OpenModComponent;
        protected readonly IConfigurationParser<TConfig> Configuration;
        protected readonly IStringLocalizer StringLocalizer;
        protected readonly IPreservationManager PreservationManager;
        protected readonly IMatchExecutor MatchExecutor;
        protected readonly IUserManager UserManager;
        protected readonly IGraceManager GraceManager;
        protected readonly ILoadoutManager LoadoutManager;
        protected readonly ILoadoutSelector LoadoutSelector;
        protected readonly IPermissionChecker PermissionChecker;
        protected readonly ILogger Logger;
        protected readonly TLoadoutCategory LoadoutCategory;

        private readonly IEventBus _eventBus;
        private readonly IEventSubscriber _eventSubscriber;
        private IDisposable? _eventSubscriptions;

        private readonly List<IGamePlayer> _players;
        private readonly AsyncLock _playersLock;

        private readonly CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Cancellation token to be used by tasks who's lifetime is of the match.
        /// </summary>
        /// <remarks>
        /// This cancellation token will be cancelled immediately prior to
        /// <see cref="OnEndAsync"/> and ending <see cref="OnPlayerRemoved"/> methods.
        /// </remarks>
        protected CancellationToken CancellationToken => _cancellationTokenSource.Token;

        protected MatchBase(IServiceProvider serviceProvider)
        {
            OpenModComponent = serviceProvider.GetRequiredService<IOpenModComponent>();
            Configuration = serviceProvider.GetRequiredService<IConfigurationParser<TConfig>>();
            StringLocalizer = serviceProvider.GetRequiredService<IStringLocalizer>();
            PreservationManager = serviceProvider.GetRequiredService<IPreservationManager>();
            MatchExecutor = serviceProvider.GetRequiredService<IMatchExecutor>();
            UserManager = serviceProvider.GetRequiredService<IUserManager>();
            GraceManager = serviceProvider.GetRequiredService<IGraceManager>();
            LoadoutManager = serviceProvider.GetRequiredService<ILoadoutManager>();
            LoadoutSelector = serviceProvider.GetRequiredService<ILoadoutSelector>();
            PermissionChecker = serviceProvider.GetRequiredService<IPermissionChecker>();
            LoadoutCategory = LoadoutManager.GetCategory<TLoadoutCategory>() ??
                              throw new Exception($"Cannot find category {nameof(TLoadoutCategory)}");

            var loggerType = typeof(ILogger<>).MakeGenericType(GetType());
            Logger = (ILogger)serviceProvider.GetRequiredService(loggerType);

            _eventBus = serviceProvider.GetRequiredService<IEventBus>();
            _eventSubscriber = serviceProvider.GetRequiredService<IEventSubscriber>();

            _players = new List<IGamePlayer>();
            _playersLock = new AsyncLock();

            _cancellationTokenSource = new CancellationTokenSource();

            var matchRegistrationAccessor = serviceProvider.GetRequiredService<IMatchRegistrationAccessor>();
            Registration = matchRegistrationAccessor.Registration;

            LifetimeScope = serviceProvider.GetRequiredService<ILifetimeScope>();

            Status = MatchStatus.Initialized;
        }

        public IMatchRegistration Registration { get; }

        public ILifetimeScope LifetimeScope { get; }

        public MatchStatus Status { get; private set; }

        public IReadOnlyCollection<IGamePlayer> Players => _players.AsReadOnly();

        public async UniTask StartAsync(IEnumerable<IGamePlayer> players)
        {
            if (Status != MatchStatus.Initialized)
            {
                return;
            }

            Status = MatchStatus.Starting;

            await UniTask.SwitchToThreadPool();

            _eventSubscriptions = _eventSubscriber.Subscribe(this, OpenModComponent);

            try
            {
                Logger.LogInformation("Starting match {Title}", Registration.Title);

                await OnStartAsync();

                await this.AddPlayers(players);

                Status = MatchStatus.InProgress;
            }
            catch
            {
                Status = MatchStatus.ExceptionWhenStarting;
                throw;
            }
        }
        
        public async UniTask EndAsync()
        {
            if (Status != MatchStatus.InProgress)
            {
                return;
            }

            Status = MatchStatus.Ending;

            await UniTask.SwitchToThreadPool();

            _eventSubscriptions?.Dispose();

            try
            {
                // Emit MatchEndingEvent

                var endingEvent = new MatchEndingEvent(this);

                await _eventBus.EmitAsync(OpenModComponent, this, endingEvent);

                // End match

                Logger.LogInformation("Ending match {Title}", Registration.Title);

                _cancellationTokenSource.Cancel();

                var players = _players.ToList();

                await this.RemovePlayers(Players);

                _players.Clear();
                _players.AddRange(players);

                await OnEndAsync();

                Status = MatchStatus.Ended;

                // Emit MatchEndedEvent

                var endedEvent = new MatchEndedEvent(this);

                await _eventBus.EmitAsync(OpenModComponent, this, endedEvent);
            }
            catch
            {
                Status = MatchStatus.ExceptionWhenEnding;
                throw;
            }

            await LifetimeScope.DisposeAsync();
        }

        /// <summary>
        /// Called when the match is starting.
        /// </summary>
        protected virtual UniTask OnStartAsync() => UniTask.CompletedTask;

        /// <summary>
        /// Called when the match is ending.
        /// </summary>
        protected virtual UniTask OnEndAsync() => UniTask.CompletedTask;

        /// <summary>
        /// Add the specified players from this match.
        /// </summary>
        /// <param name="players">The players to add.</param>
        public async UniTask AddPlayers(params IGamePlayer[] players)
        {
            switch (Status)
            {
                case MatchStatus.Ending:
                    throw new Exception("Cannot add players. Match is ending.");
                case MatchStatus.Ended:
                    throw new Exception("Cannot add players. Match has ended.");
                case MatchStatus.ExceptionWhenStarting:
                case MatchStatus.ExceptionWhenEnding:
                    throw new Exception("Cannot add players. Match is in exception state.");
            }

            using (await _playersLock.LockAsync())
            {
                foreach (var player in players)
                {
                    if (_players.Contains(player))
                    {
                        continue;
                    }

                    _players.Add(player);

                    await OnPlayerAddedInternal(player);

                    await OnPlayerAdded(player);
                }
            }
        }

        /// <summary>
        /// Remove the specified players from this match.
        /// </summary>
        /// <param name="players">The players to remove.</param>
        public async UniTask RemovePlayers(params IGamePlayer[] players)
        {
            using (await _playersLock.LockAsync())
            {
                foreach (var player in players)
                {
                    if (_players.Remove(player))
                    {
                        await OnPlayerRemovedInternal(player);

                        await OnPlayerRemoved(player);
                    }
                }
            }
        }

        private UniTask OnPlayerAddedInternal(IGamePlayer player)
        {
            player.ClearMatchData();

            player.CurrentMatch = this;

            return UniTask.CompletedTask;
        }

        private UniTask OnPlayerRemovedInternal(IGamePlayer player)
        {
            player.ClearMatchData();

            player.CurrentMatch = null;

            return UniTask.CompletedTask;
        }

        /// <summary>
        /// This method is called when a player is added to the match.
        /// This includes when the match starts and all players are added.
        /// </summary>
        /// <remarks>
        /// When the match is starting, <see cref="OnStartAsync"/> is called before this method. 
        /// </remarks>
        /// <param name="player">The player added.</param>
        protected virtual UniTask OnPlayerAdded(IGamePlayer player) => UniTask.CompletedTask;

        /// <summary>
        /// This method is called when a player is removed from the match.
        /// This includes when the match ends and all players are added.
        /// </summary>
        /// <remarks>
        /// When the match is ending, <see cref="OnEndAsync"/> is called after this method.
        /// </remarks>
        /// <param name="player">The player removed.</param>
        protected virtual UniTask OnPlayerRemoved(IGamePlayer player) => UniTask.CompletedTask;

        public async ValueTask DisposeAsync()
        {
            await EndAsync();
        }
    }
}
