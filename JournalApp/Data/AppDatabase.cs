using SQLite;
using JournalApp.Models;

namespace JournalApp.Data
{
    public class AppDatabase
    {
        private readonly SQLiteAsyncConnection _database;

        public AppDatabase(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
        }

        // =========================
        // INITIALIZATION
        // =========================

        public async Task InitializeAsync()
        {
            await _database.CreateTableAsync<Mood>();
            await _database.CreateTableAsync<JournalEntry>();

            await SeedMoodsIfEmpty();
        }

        // =========================
        // MOODS
        // =========================

        public Task<List<Mood>> GetMoodsAsync()
        {
            return _database.Table<Mood>().ToListAsync();
        }

        private async Task SeedMoodsIfEmpty()
        {
            var count = await _database.Table<Mood>().CountAsync();
            if (count == 0)
            {
                await _database.InsertAllAsync(new[]
                {
                    new Mood { Name = "Happy", Category = "Positive" },
                    new Mood { Name = "Calm", Category = "Positive" },
                    new Mood { Name = "Sad", Category = "Negative" },
                    new Mood { Name = "Stressed", Category = "Negative" }
                });
            }
        }

        // =========================
        // JOURNAL
        // =========================

        public Task<List<JournalEntry>> GetEntriesAsync()
        {
            return _database.Table<JournalEntry>()
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public Task<int> AddEntryAsync(JournalEntry entry)
        {
            return _database.InsertAsync(entry);
        }
    }
}
