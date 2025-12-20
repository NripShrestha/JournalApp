using JournalApp.Data;

namespace JournalApp
{
    public partial class MainPage : ContentPage
    {
        private readonly AppDatabase _database;

        public MainPage(AppDatabase database)
        {
            InitializeComponent();
            _database = database;

            _ = InitializeDatabaseAsync();
        }

        private async Task InitializeDatabaseAsync()
        {
            await _database.InitializeAsync();
        }
    }
}
