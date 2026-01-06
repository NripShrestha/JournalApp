using System.Security.Cryptography;
using System.Text;
using JournalApp.Data;
using JournalApp.Models;

namespace JournalApp.Data
{
    public class SecurityService
    {
        private readonly AppDatabase _db;

        public bool IsUnlocked { get; private set; }
        public string CurrentUsername { get; private set; } = string.Empty;

        public SecurityService(AppDatabase db)
        {
            _db = db;
        }

        // Check if app has been set up
        public async Task<bool> IsSetupCompleteAsync()
        {
            var sec = await _db.GetSecurityAsync();
            return sec != null && !string.IsNullOrEmpty(sec.PinHash);
        }

        // Initial setup
        public async Task SetupAccountAsync(string username, string pin, string securityQuestion, string securityAnswer)
        {
            await _db.SaveSecurityAsync(new AppSecurity
            {
                Username = username,
                PinHash = Hash(pin),
                SecurityQuestion = securityQuestion,
                SecurityAnswerHash = Hash(securityAnswer.ToLower().Trim()),
                CreatedAt = DateTime.Now
            });

            CurrentUsername = username;
            IsUnlocked = true;
        }

        // Login verification
        public async Task<bool> VerifyPinAsync(string pin)
        {
            var sec = await _db.GetSecurityAsync();
            if (sec == null) return false;

            var hash = Hash(pin);
            IsUnlocked = hash == sec.PinHash;

            if (IsUnlocked)
            {
                CurrentUsername = sec.Username;
            }

            return IsUnlocked;
        }

        // Get username
        public async Task<string> GetUsernameAsync()
        {
            var sec = await _db.GetSecurityAsync();
            return sec?.Username ?? string.Empty;
        }

        // Get security question
        public async Task<string> GetSecurityQuestionAsync()
        {
            var sec = await _db.GetSecurityAsync();
            return sec?.SecurityQuestion ?? string.Empty;
        }

        // Verify security answer
        public async Task<bool> VerifySecurityAnswerAsync(string answer)
        {
            var sec = await _db.GetSecurityAsync();
            if (sec == null) return false;

            var hash = Hash(answer.ToLower().Trim());
            return hash == sec.SecurityAnswerHash;
        }

        // Reset PIN after security question verification
        public async Task ResetPinAsync(string newPin)
        {
            var sec = await _db.GetSecurityAsync();
            if (sec == null) return;

            sec.PinHash = Hash(newPin);
            await _db.UpdateSecurityAsync(sec);
        }

        // Logout
        public void Logout()
        {
            IsUnlocked = false;
            CurrentUsername = string.Empty;
        }

        private string Hash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }
    }
}