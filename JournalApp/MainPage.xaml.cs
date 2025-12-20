using JournalApp.Data;

namespace JournalApp
{
    public partial class MainPage : ContentPage
    {
        private readonly AppDatabase _database;
        private bool _initialized;

        public MainPage(AppDatabase database)
        {
            InitializeComponent();
            _database = database;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_initialized)
                return;

            _initialized = true;

            // SAFE async initialization
            await _database.InitializeAsync();
        }
    }
}
