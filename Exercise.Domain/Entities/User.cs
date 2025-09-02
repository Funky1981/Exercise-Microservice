using System;
using System.Text.RegularExpressions;
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

            PasswordHash = $"HASHED_{password}"; // TODO: Implement proper hashing
        }

        public void UpdateProfile(string? userName, Height? height, Weight? weight)
        {
            if (!string.IsNullOrWhiteSpace(userName))
                UserName = userName;
            
            Height = height;
            Weight = weight;
        }

        private static bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
            return emailRegex.IsMatch(email);
        }

        public override string ToString()
        {
            return $"{Name} ({Email})";
        }
    }
}
// Note: The PasswordHash should be stored securely using a proper hashing algorithm.