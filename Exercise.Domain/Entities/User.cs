using System;
using System.Net.Mail;
using Exercise.Domain.ValueObjects;
using Exercise.Domain.Common;

namespace Exercise.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Email { get; private set; }
        public string? PasswordHash { get; private set; }
        public string? Provider { get; private set; }
        public string? ProviderId { get; private set; }
        public string Name { get; private set; }
        public string? UserName { get; private set; }
        public Height? Height { get; private set; }
        public Weight? Weight { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string? RefreshTokenHash { get; private set; }
        public string? RefreshTokenPreviousHash { get; private set; }   // for reuse detection
        public DateTime? RefreshTokenExpiry { get; private set; }

        private User() { } // For ORM

        public User(Guid id, string name, string email, string? provider = null, string? providerId = null)
        {
            Guard.AgainstEmptyGuid(id, nameof(id));
            Guard.AgainstNullOrWhiteSpace(name, nameof(name));
            Guard.AgainstNullOrWhiteSpace(email, nameof(email));
            
            if (!IsValidEmail(email))
                throw new ArgumentException("Email format is invalid.", nameof(email));

            Id = id;
            Name = name;
            Email = email;
            Provider = provider;
            ProviderId = providerId;
            CreatedAt = DateTime.UtcNow;
        }

        public bool IsSocialLogin()
        {
            return !string.IsNullOrWhiteSpace(Provider);
        }

        public void SetPassword(string password)
        {
            if (IsSocialLogin())
                throw new InvalidOperationException("Cannot set a password for a user with an external provider.");

            Guard.AgainstNullOrWhiteSpace(password, nameof(password));

            if (password.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters long.", nameof(password));

            if (!password.Any(char.IsUpper))
                throw new ArgumentException("Password must contain at least one uppercase letter.", nameof(password));

            if (!password.Any(char.IsLower))
                throw new ArgumentException("Password must contain at least one lowercase letter.", nameof(password));

            if (!password.Any(char.IsDigit))
                throw new ArgumentException("Password must contain at least one digit.", nameof(password));

            if (!password.Any(c => !char.IsLetterOrDigit(c)))
                throw new ArgumentException("Password must contain at least one special character.", nameof(password));

            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Verifies a plain-text password against the stored BCrypt hash.
        /// </summary>
        public bool VerifyPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(PasswordHash))
                return false;

            return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
        }

        public void UpdateProfile(string? userName, Height? height, Weight? weight)        {
            if (!string.IsNullOrWhiteSpace(userName))
                UserName = userName;
            
            Height = height;
            Weight = weight;
        }

        public void SetRefreshToken(string token, DateTime expiry)
        {
            // Preserve current hash so reuse of the old token can be detected.
            RefreshTokenPreviousHash = RefreshTokenHash;
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(token);
            RefreshTokenHash = Convert.ToHexString(sha.ComputeHash(bytes));
            RefreshTokenExpiry = expiry;
        }

        /// <summary>
        /// Returns true when the presented token matches the PREVIOUS (already-rotated)
        /// hash, indicating a token-reuse / replay attack.
        /// When true the caller MUST revoke all tokens for this user.
        /// </summary>
        public bool IsRefreshTokenReused(string token)
        {
            if (string.IsNullOrWhiteSpace(RefreshTokenPreviousHash))
                return false;

            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(token);
            var hash  = Convert.ToHexString(sha.ComputeHash(bytes));
            return hash.Equals(RefreshTokenPreviousHash, StringComparison.OrdinalIgnoreCase);
        }

        public bool VerifyRefreshToken(string token)
        {
            if (string.IsNullOrWhiteSpace(RefreshTokenHash) || RefreshTokenExpiry <= DateTime.UtcNow)
                return false;

            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(token);
            var hash = Convert.ToHexString(sha.ComputeHash(bytes));
            return hash.Equals(RefreshTokenHash, StringComparison.OrdinalIgnoreCase);
        }

        public void ClearRefreshToken()
        {
            RefreshTokenHash         = null;
            RefreshTokenPreviousHash = null;
            RefreshTokenExpiry       = null;
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public override string ToString()
        {
            return $"{Name} ({Email})";
        }
    }
}
// Note: The PasswordHash should be stored securely using a proper hashing algorithm.