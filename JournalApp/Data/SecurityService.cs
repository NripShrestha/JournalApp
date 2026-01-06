using System.Security.Cryptography;
using System.Text;
using JournalApp.Data;

namespace JournalApp.Data
{
    public class SecurityService
    {
        private readonly AppDatabase _db;

        public bool IsUnlocked { get; private set; }

        public SecurityService(AppDatabase db)
        {
            _db = db;
        }

        public async Task<bool> HasPinAsync()
        {
            var sec = await _db.GetSecurityAsync();
            return sec != null;
        }

        public async Task<bool> VerifyPinAsync(string pin)
        {
            var sec = await _db.GetSecurityAsync();
            if (sec == null) return false;

            var hash = Hash(pin);
            IsUnlocked = hash == sec.PinHash;
            return IsUnlocked;
        }

        public async Task SetPinAsync(string pin)
        {
            await _db.SavePinHashAsync(Hash(pin));
            IsUnlocked = true;
        }

        private string Hash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }
    }
}
