using SQLite;

namespace JournalApp.Models
{
    public class AppSecurity
    {
        [PrimaryKey]
        public int Id { get; set; } = 1;

        public string PinHash { get; set; } = string.Empty;
    }
}
