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
            await _database.CreateTableAsync<JournalEntryMood>();

            await _database.CreateTableAsync<Tag>();
            await _database.CreateTableAsync<JournalEntryTag>();

            await SeedMoodsIfEmpty();
            await SeedTagsIfEmpty();
        }


        // =========================
        // JOURNAL MOODS
        // =========================

        public async Task<List<Mood>> GetMoodsForEntryAsync(int journalEntryId)
        {
            // Step 1: Get mood IDs linked to the journal entry
            var moodLinks = await _database.Table<JournalEntryMood>()
                .Where(jm => jm.JournalEntryId == journalEntryId)
                .ToListAsync();

            if (moodLinks.Count == 0)
                return new List<Mood>();

            var moodIds = moodLinks.Select(jm => jm.MoodId).ToList();

            // Step 2: Fetch moods by ID
            var moods = await _database.Table<Mood>()
                .Where(m => moodIds.Contains(m.Id))
                .ToListAsync();

            return moods;
        }

        public async Task SaveEntryMoodsAsync(
            int journalEntryId,
            int primaryMoodId,
            List<int> secondaryMoodIds)
        {
            // Remove existing moods
            var existing = await _database.Table<JournalEntryMood>()
                .Where(jm => jm.JournalEntryId == journalEntryId)
                .ToListAsync();

            foreach (var item in existing)
                await _database.DeleteAsync(item);

            // Insert primary mood
            await _database.InsertAsync(new JournalEntryMood
            {
                JournalEntryId = journalEntryId,
                MoodId = primaryMoodId,
                IsPrimary = true
            });

            // Insert secondary moods (max 2)
            foreach (var moodId in secondaryMoodIds.Take(2))
            {
                await _database.InsertAsync(new JournalEntryMood
                {
                    JournalEntryId = journalEntryId,
                    MoodId = moodId,
                    IsPrimary = false
                });
            }
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
        private async Task SeedTagsIfEmpty()
        {
            var count = await _database.Table<Tag>().CountAsync();
            if (count == 0)
            {
                await _database.InsertAllAsync(new[]
                {
            new Tag { Name = "Work", IsPredefined = true },
            new Tag { Name = "Health", IsPredefined = true },
            new Tag { Name = "Travel", IsPredefined = true },
            new Tag { Name = "Family", IsPredefined = true },
            new Tag { Name = "Fitness", IsPredefined = true },
            new Tag { Name = "Study", IsPredefined = true }
        });
            }
        }
        public Task<List<Tag>> GetTagsAsync()
        {
            return _database.Table<Tag>()
                .OrderBy(t => t.Name)
                .ToListAsync();
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
        public async Task SaveEntryTagsAsync(
    int journalEntryId,
    List<int> tagIds)
        {
            // Remove existing tags
            var existing = await _database.Table<JournalEntryTag>()
                .Where(jt => jt.JournalEntryId == journalEntryId)
                .ToListAsync();

            foreach (var item in existing)
                await _database.DeleteAsync(item);

            // Insert new tags
            foreach (var tagId in tagIds)
            {
                await _database.InsertAsync(new JournalEntryTag
                {
                    JournalEntryId = journalEntryId,
                    TagId = tagId
                });
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
        public async Task<List<Tag>> GetTagsForEntryAsync(int journalEntryId)
        {
            // Step 1: Get tag IDs linked to this entry
            var tagLinks = await _database.Table<JournalEntryTag>()
                .Where(jt => jt.JournalEntryId == journalEntryId)
                .ToListAsync();

            if (tagLinks.Count == 0)
                return new List<Tag>();

            var tagIds = tagLinks.Select(jt => jt.TagId).ToList();

            // Step 2: Fetch actual tags
            return await _database.Table<Tag>()
                .Where(t => tagIds.Contains(t.Id))
                .ToListAsync();
        }
        // Get mood links (needed to distinguish primary vs secondary)
        public async Task<List<JournalEntryMood>> GetMoodLinksForEntryAsync(int journalEntryId)
        {
            return await _database.Table<JournalEntryMood>()
                .Where(jm => jm.JournalEntryId == journalEntryId)
                .ToListAsync();
        }

        // Update existing entry
        public Task UpdateEntryAsync(JournalEntry entry)
        {
            return _database.UpdateAsync(entry);
        }

    }
}
