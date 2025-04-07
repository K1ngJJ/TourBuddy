using TourBuddy.Services.Database;
namespace TourBuddy
{
    public partial class App : Application
    {
        private readonly ISQLiteService _sqliteService;
        private readonly SyncService _syncService;
        public App(ISQLiteService sqliteService, SyncService syncService)
        {
            InitializeComponent();
            _sqliteService = sqliteService;
            _syncService = syncService;

            MainPage = new AppShell();
        }
        protected override async void OnStart()
        {
            base.OnStart();

            // Initialize local database
            await _sqliteService.InitializeDatabaseAsync();

            // Start auto sync in background if enabled
            await _syncService.StartAutoSyncIfEnabledAsync();
        }
    }
}
