using Cysharp.Threading.Tasks;
using Deathmatch.API.Matches;
using Deathmatch.API.Preservation;
using Deathmatch.Core.Configuration;
using Deathmatch.Core.Helpers;
using Deathmatch.Core.Matches;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OpenMod.API.Plugins;
using OpenMod.API.Users;
using OpenMod.Core.Users;
using OpenMod.Unturned.Plugins;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

[assembly: PluginMetadata("Deathmatch.Core", DisplayName = "Deathmatch")]
namespace Deathmatch.Core
{
    public class DeathmatchPlugin : OpenModUnturnedPlugin
    {
        private readonly IMatchManager _matchManager;
        private readonly IPluginAssemblyStore _pluginAssemblyStore;
        private readonly IMatchExecutor _matchExecutor;
        private readonly IPreservationManager _preservationManager;
        private readonly IUserManager _userManager;
        private readonly ILogger<DeathmatchPlugin> _logger;
        private readonly IConfiguration _configuration;
        private readonly IStringLocalizer _stringLocalizer;
        private readonly IServiceProvider _serviceProvider;

        private CancellationTokenSource _cancellationTokenSource;

        private const string MatchesConfig = "matches";

        public DeathmatchPlugin(IMatchManager matchManager,
            IPluginAssemblyStore pluginAssemblyStore,
            IMatchExecutor matchExecutor,
            IPreservationManager preservationManager,
            IUserManager userManager,
            ILogger<DeathmatchPlugin> logger,
            IConfiguration configuration,
            IStringLocalizer stringLocalizer,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _matchManager = matchManager;
            _pluginAssemblyStore = pluginAssemblyStore;
            _matchExecutor = matchExecutor;
            _preservationManager = preservationManager;
            _userManager = userManager;
            _logger = logger;
            _configuration = configuration;
            _stringLocalizer = stringLocalizer;
            _serviceProvider = serviceProvider;

            _cancellationTokenSource = new CancellationTokenSource();
        }

        protected override async UniTask OnLoadAsync()
        {
            foreach (var pluginAssembly in _pluginAssemblyStore.LoadedPluginAssemblies)
            {
                var matchProvider =
                    ActivatorUtilities.CreateInstance<AssemblyMatchProvider>(_serviceProvider, pluginAssembly);

                if (matchProvider.GetMatchRegistrations().Count == 0) continue;

                _matchManager.AddMatchProvider(matchProvider);
            }

            var matchRegistrations = _matchManager.GetMatchRegistrations();

            var matchesConfig = await DataStore.ExistsAsync(MatchesConfig)
                ? await DataStore.LoadAsync<List<RegisteredMatchInfo>>(MatchesConfig)
                : null;

            matchesConfig ??= new List<RegisteredMatchInfo>();

            foreach (var info in matchesConfig)
            {
                var registration =
                    matchRegistrations.FirstOrDefault(x => info.Id.Equals(x.Id, StringComparison.OrdinalIgnoreCase));

                if (registration == null) continue;

                info.ApplyTo(registration);
            }

            foreach (var registration in matchRegistrations.OrderBy(x => x.Id))
            {
                if (matchesConfig.Any(x => registration.Id.Equals(x.Id, StringComparison.OrdinalIgnoreCase))) continue;

                matchesConfig.Add(new RegisteredMatchInfo(registration));
            }

            await DataStore.SaveAsync(MatchesConfig, matchesConfig);

            foreach (var registration in matchRegistrations)
            {
                _logger.LogInformation(_stringLocalizer["logs:registered_match", new { Registration = registration }]);
            }

            _cancellationTokenSource = new CancellationTokenSource();

            Level.onPostLevelLoaded += MatchLoopStarter;

            if (Level.isLoaded)
                MatchLoopStarter(0);
        }

        protected override async UniTask OnUnloadAsync()
        {
            await UniTask.SwitchToMainThread();

            // ReSharper disable once DelegateSubtraction
            Level.onPostLevelLoaded -= MatchLoopStarter;

            _cancellationTokenSource.Cancel();

            await _matchExecutor.EndMatch();

            var participants = _matchExecutor.GetParticipants();

            for (int i = participants.Count - 1; i >= 0; i--)
            {
                await _matchExecutor.RemoveParticipant(participants.ElementAt(i));
            }

            await _preservationManager.RestoreAll();
        }

        private void MatchLoopStarter(int level)
        {
            MatchAutoStartLoop().Forget();
        }

        private bool _loopStarted;

        private UniTask DelaySeconds(int seconds) => UniTask.Delay(seconds * 1000, cancellationToken: _cancellationTokenSource.Token);

        private async UniTask MatchAutoStartLoop()
        {
            if (_loopStarted)
            {
                return;
            }

            _loopStarted = true;

            var delay = _configuration.GetValue<int>("MatchInterval");

            if (delay <= 0)
            {
                return;
            }

            var announcements = _configuration.GetSection("AutoAnnouncements").Get<List<AutoAnnouncement>>() ??
                                new List<AutoAnnouncement>();

            // Sorts descending
            announcements.Sort((x, y) => y.SecondsBefore.CompareTo(x.SecondsBefore));

            announcements.RemoveAll(x => x.SecondsBefore > delay);

            announcements.Add(new AutoAnnouncement
            {
                SecondsBefore = 0,
                MessageTime = null
            });

            var delays = new List<(int,string?)>();

            for (var i = 0; i < announcements.Count; i++)
            {
                var a = announcements[i];

                var prevDelay = i == 0 ? delay : announcements[i - 1].SecondsBefore;

                delays.Add((prevDelay - a.SecondsBefore, a.MessageTime));
            }

            // Always wait thirty seconds
            await DelaySeconds(30);

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                IMatchRegistration? registration = null;

                if (_matchExecutor.CurrentMatch == null || !_matchExecutor.CurrentMatch.IsRunning)
                {
                    var registrations = _matchManager.GetMatchRegistrations();

                    if (registrations.Count == 0)
                    {
                        _logger.LogCritical(_stringLocalizer["logs:no_registrations"]);
                    }
                    else
                    {
                        registration = registrations.RandomElement();
                    }
                }

                if (registration == null)
                {
                    await DelaySeconds(delay);
                }
                else
                {
                    foreach (var (delayPart, message) in delays)
                    {
                        await DelaySeconds(delayPart);

                        if (message != null)
                            await _userManager.BroadcastAsync(KnownActorTypes.Player,
                                _stringLocalizer["announcements:planned_match",
                                    new { Match = registration, Time = message }]);
                    }

                    await _matchExecutor.StartMatch(registration);
                }
            }
        }
    }
}
