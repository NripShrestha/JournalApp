using SQLite;

namespace JournalApp.Models
{
    public class JournalEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        // Used to enforce one entry per day
        public DateTime EntryDate { get; set; }

        // System timestamps
        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
