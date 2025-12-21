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

            // Log counts after seeding
            var moodCount = await _database.Table<Mood>().CountAsync();
            var tagCount = await _database.Table<Tag>().CountAsync();

            
        }

        // =========================
        // MOODS - COMPLETE SEEDING
        // =========================

        private async Task SeedMoodsIfEmpty()
        {
            var count = await _database.Table<Mood>().CountAsync();

            // If we have less than 15 moods, add the missing ones
            if (count < 15)
            {
                var existingMoods = await _database.Table<Mood>().ToListAsync();
                var existingNames = existingMoods.Select(m => m.Name).ToHashSet();

                var allMoods = new[]
                {
                    // Positive Moods
                    new Mood { Name = "Happy", Category = "Positive" },
                    new Mood { Name = "Excited", Category = "Positive" },
                    new Mood { Name = "Relaxed", Category = "Positive" },
                    new Mood { Name = "Grateful", Category = "Positive" },
                    new Mood { Name = "Confident", Category = "Positive" },
                    
                    // Neutral Moods
                    new Mood { Name = "Calm", Category = "Neutral" },
                    new Mood { Name = "Thoughtful", Category = "Neutral" },
                    new Mood { Name = "Curious", Category = "Neutral" },
                    new Mood { Name = "Nostalgic", Category = "Neutral" },
                    new Mood { Name = "Bored", Category = "Neutral" },
                    
                    // Negative Moods
                    new Mood { Name = "Sad", Category = "Negative" },
                    new Mood { Name = "Angry", Category = "Negative" },
                    new Mood { Name = "Stressed", Category = "Negative" },
                    new Mood { Name = "Lonely", Category = "Negative" },
                    new Mood { Name = "Anxious", Category = "Negative" }
                };

                // Only insert moods that don't exist
                var moodsToAdd = allMoods.Where(m => !existingNames.Contains(m.Name)).ToList();

                if (moodsToAdd.Any())
                {
                    await _database.InsertAllAsync(moodsToAdd);
                    
                }
            }
        }

        public Task<List<Mood>> GetMoodsAsync()
        {
            return _database.Table<Mood>().ToListAsync();
        }

        public async Task<List<Mood>> GetMoodsByCategoryAsync(string category)
        {
            return await _database.Table<Mood>()
                .Where(m => m.Category == category)
                .ToListAsync();
        }

        // =========================
        // JOURNAL MOODS
        // =========================

        public async Task<List<Mood>> GetMoodsForEntryAsync(int journalEntryId)
        {
            var moodLinks = await _database.Table<JournalEntryMood>()
                .Where(jm => jm.JournalEntryId == journalEntryId)
                .ToListAsync();

            if (moodLinks.Count == 0)
                return new List<Mood>();

            var moodIds = moodLinks.Select(jm => jm.MoodId).ToList();

            var moods = await _database.Table<Mood>()
                .Where(m => moodIds.Contains(m.Id))
                .ToListAsync();

            return moods;
        }

        public async Task<List<JournalEntryMood>> GetMoodLinksForEntryAsync(int journalEntryId)
        {
            return await _database.Table<JournalEntryMood>()
                .Where(jm => jm.JournalEntryId == journalEntryId)
                .ToListAsync();
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
        // MOOD ANALYTICS
        // =========================

        public async Task<Dictionary<string, int>> GetMoodCategoryDistributionAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var allEntries = await _database.Table<JournalEntry>().ToListAsync();

            if (startDate.HasValue)
                allEntries = allEntries.Where(e => e.EntryDate >= startDate.Value.Date).ToList();

            if (endDate.HasValue)
                allEntries = allEntries.Where(e => e.EntryDate <= endDate.Value.Date).ToList();

            var distribution = new Dictionary<string, int>
            {
                { "Positive", 0 },
                { "Neutral", 0 },
                { "Negative", 0 }
            };

            foreach (var entry in allEntries)
            {
                var moodLinks = await _database.Table<JournalEntryMood>()
                    .Where(jm => jm.JournalEntryId == entry.Id && jm.IsPrimary)
                    .FirstOrDefaultAsync();

                if (moodLinks != null)
                {
                    var mood = await _database.Table<Mood>()
                        .Where(m => m.Id == moodLinks.MoodId)
                        .FirstOrDefaultAsync();

                    if (mood != null && distribution.ContainsKey(mood.Category))
                    {
                        distribution[mood.Category]++;
                    }
                }
            }

            return distribution;
        }

        public async Task<Mood?> GetMostFrequentMoodAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var allEntries = await _database.Table<JournalEntry>().ToListAsync();

            if (startDate.HasValue)
                allEntries = allEntries.Where(e => e.EntryDate >= startDate.Value.Date).ToList();

            if (endDate.HasValue)
                allEntries = allEntries.Where(e => e.EntryDate <= endDate.Value.Date).ToList();

            var moodCounts = new Dictionary<int, int>();

            foreach (var entry in allEntries)
            {
                var moodLinks = await _database.Table<JournalEntryMood>()
                    .Where(jm => jm.JournalEntryId == entry.Id && jm.IsPrimary)
                    .FirstOrDefaultAsync();

                if (moodLinks != null)
                {
                    if (moodCounts.ContainsKey(moodLinks.MoodId))
                        moodCounts[moodLinks.MoodId]++;
                    else
                        moodCounts[moodLinks.MoodId] = 1;
                }
            }

            if (moodCounts.Count == 0)
                return null;

            var mostFrequentMoodId = moodCounts.OrderByDescending(kvp => kvp.Value).First().Key;

            return await _database.Table<Mood>()
                .Where(m => m.Id == mostFrequentMoodId)
                .FirstOrDefaultAsync();
        }

        // =========================
        // TAGS
        // =========================

        private async Task SeedTagsIfEmpty()
        {
            var count = await _database.Table<Tag>().CountAsync();

            // If we have less than 31 tags, add the missing ones
            if (count < 31)
            {
                var existingTags = await _database.Table<Tag>().ToListAsync();
                var existingNames = existingTags.Select(t => t.Name).ToHashSet();

                var allTags = new[]
                {
                    new Tag { Name = "Work", IsPredefined = true },
                    new Tag { Name = "Career", IsPredefined = true },
                    new Tag { Name = "Studies", IsPredefined = true },
                    new Tag { Name = "Family", IsPredefined = true },
                    new Tag { Name = "Friends", IsPredefined = true },
                    new Tag { Name = "Relationships", IsPredefined = true },
                    new Tag { Name = "Health", IsPredefined = true },
                    new Tag { Name = "Fitness", IsPredefined = true },
                    new Tag { Name = "Personal Growth", IsPredefined = true },
                    new Tag { Name = "Self-care", IsPredefined = true },
                    new Tag { Name = "Hobbies", IsPredefined = true },
                    new Tag { Name = "Travel", IsPredefined = true },
                    new Tag { Name = "Nature", IsPredefined = true },
                    new Tag { Name = "Finance", IsPredefined = true },
                    new Tag { Name = "Spirituality", IsPredefined = true },
                    new Tag { Name = "Birthday", IsPredefined = true },
                    new Tag { Name = "Holiday", IsPredefined = true },
                    new Tag { Name = "Vacation", IsPredefined = true },
                    new Tag { Name = "Celebration", IsPredefined = true },
                    new Tag { Name = "Exercise", IsPredefined = true },
                    new Tag { Name = "Reading", IsPredefined = true },
                    new Tag { Name = "Writing", IsPredefined = true },
                    new Tag { Name = "Cooking", IsPredefined = true },
                    new Tag { Name = "Meditation", IsPredefined = true },
                    new Tag { Name = "Yoga", IsPredefined = true },
                    new Tag { Name = "Music", IsPredefined = true },
                    new Tag { Name = "Shopping", IsPredefined = true },
                    new Tag { Name = "Parenting", IsPredefined = true },
                    new Tag { Name = "Projects", IsPredefined = true },
                    new Tag { Name = "Planning", IsPredefined = true },
                    new Tag { Name = "Reflection", IsPredefined = true }
                };

                // Only insert tags that don't exist
                var tagsToAdd = allTags.Where(t => !existingNames.Contains(t.Name)).ToList();

                if (tagsToAdd.Any())
                {
                    await _database.InsertAllAsync(tagsToAdd);
                    
                }
            }
        }

        public Task<List<Tag>> GetTagsAsync()
        {
            return _database.Table<Tag>()
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<List<Tag>> GetTagsForEntryAsync(int journalEntryId)
        {
            var tagLinks = await _database.Table<JournalEntryTag>()
                .Where(jt => jt.JournalEntryId == journalEntryId)
                .ToListAsync();

            if (tagLinks.Count == 0)
                return new List<Tag>();

            var tagIds = tagLinks.Select(jt => jt.TagId).ToList();

            return await _database.Table<Tag>()
                .Where(t => tagIds.Contains(t.Id))
                .ToListAsync();
        }

        public async Task SaveEntryTagsAsync(int journalEntryId, List<int> tagIds)
        {
            var existing = await _database.Table<JournalEntryTag>()
                .Where(jt => jt.JournalEntryId == journalEntryId)
                .ToListAsync();

            foreach (var item in existing)
                await _database.DeleteAsync(item);

            foreach (var tagId in tagIds)
            {
                await _database.InsertAsync(new JournalEntryTag
                {
                    JournalEntryId = journalEntryId,
                    TagId = tagId
                });
            }
        }

        // =========================
        // JOURNAL ENTRIES
        // =========================

        public Task<JournalEntry?> GetEntryByDateAsync(DateTime date)
        {
            return _database.Table<JournalEntry>()
                .Where(e => e.EntryDate == date.Date)
                .FirstOrDefaultAsync();
        }

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

        public Task<List<JournalEntry>> GetEntriesAsync()
        {
            return _database.Table<JournalEntry>()
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<JournalEntry>> GetEntriesFilteredAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            List<int>? moodIds = null,
            List<int>? tagIds = null)
        {
            var allEntries = await _database.Table<JournalEntry>().ToListAsync();

            // Filter by date range
            if (startDate.HasValue)
                allEntries = allEntries.Where(e => e.EntryDate >= startDate.Value.Date).ToList();

            if (endDate.HasValue)
                allEntries = allEntries.Where(e => e.EntryDate <= endDate.Value.Date).ToList();

            // Filter by moods
            if (moodIds != null && moodIds.Count > 0)
            {
                var filteredByMood = new List<JournalEntry>();
                foreach (var entry in allEntries)
                {
                    var entryMoodLinks = await _database.Table<JournalEntryMood>()
                        .Where(jm => jm.JournalEntryId == entry.Id)
                        .ToListAsync();

                    if (entryMoodLinks.Any(jm => moodIds.Contains(jm.MoodId)))
                    {
                        filteredByMood.Add(entry);
                    }
                }
                allEntries = filteredByMood;
            }

            // Filter by tags
            if (tagIds != null && tagIds.Count > 0)
            {
                var filteredByTag = new List<JournalEntry>();
                foreach (var entry in allEntries)
                {
                    var entryTagLinks = await _database.Table<JournalEntryTag>()
                        .Where(jt => jt.JournalEntryId == entry.Id)
                        .ToListAsync();

                    if (entryTagLinks.Any(jt => tagIds.Contains(jt.TagId)))
                    {
                        filteredByTag.Add(entry);
                    }
                }
                allEntries = filteredByTag;
            }

            return allEntries.OrderByDescending(e => e.CreatedAt).ToList();
        }

        public Task UpdateEntryAsync(JournalEntry entry)
        {
            return _database.UpdateAsync(entry);
        }

        public Task DeleteEntryAsync(JournalEntry entry)
        {
            return _database.DeleteAsync(entry);
        }

        public Task<int> AddEntryAsync(JournalEntry entry)
        {
            return _database.InsertAsync(entry);
        }
    }
}