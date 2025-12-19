using SQLite;

namespace JournalApp.Models
{
    public class JournalEntryMood
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int JournalEntryId { get; set; }

        public int MoodId { get; set; }

        // True = Primary mood, False = Secondary mood
        public bool IsPrimary { get; set; }
    }
}
