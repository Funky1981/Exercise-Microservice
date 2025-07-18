using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? Provider { get; set; }      // e.g., "Google", "Facebook"
        public string? ProviderId { get; set; }    // External provider's user ID
        public string Name { get; set; }
        public string? UserName { get; set; }
        public string? Height { get; set; }
        public string? Weight { get; set; }       
        public DateTime CreatedAt { get; set; }
        public User(Guid id, string name, string email)
        {
            Id = id;
            Name = name;
            Email = email;
            CreatedAt = DateTime.Now;
        }
        public override string ToString()
        {
            return $"{Name} ({Email})";
        }
    }
}
