using SQLite;

namespace JournalApp.Models
{
    public class AppSecurity
    {
        [PrimaryKey]
        public int Id { get; set; } = 1;

        public string Username { get; set; } = string.Empty;

        public string PinHash { get; set; } = string.Empty;

        // NEW
        public string SchoolNameHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
