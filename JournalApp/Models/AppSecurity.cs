using SQLite;

namespace JournalApp.Models
{
    public class AppSecurity
    {
        [PrimaryKey]
        public int Id { get; set; } = 1;

        public string Username { get; set; } = string.Empty;

        public string PinHash { get; set; } = string.Empty;

        public string SecurityQuestion { get; set; } = string.Empty;

        public string SecurityAnswerHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}