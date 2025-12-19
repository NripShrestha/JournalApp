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
        // Get entry for a specific date (one per day rule)
        public Task<JournalEntry?> GetEntryByDateAsync(DateTime date)
        {
            return _database.Table<JournalEntry>()
                .Where(e => e.EntryDate == date.Date)
                .FirstOrDefaultAsync();
        }

        // Insert or update today's entry
        public async Task SaveEntryAsync(JournalEntry entry)
        {
            var existing = await GetEntryByDateAsync(entry.EntryDate);

            if (existing == null)
            {
                entry.CreatedAt = DateTime.Now;
                entry.UpdatedAt = DateTime.Now;
                await _database.InsertAsync(entry);
            }
            else
            {
                existing.Title = entry.Title;
                existing.Content = entry.Content;
                existing.UpdatedAt = DateTime.Now;
                await _database.UpdateAsync(existing);
            }
        }

        // Delete entry by id
        public Task DeleteEntryAsync(JournalEntry entry)
        {
            return _database.DeleteAsync(entry);
        }

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
