using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise.Domain.Entities
{
    public class Analytics
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public string Exercises { get; set; } // JSON string of exercises
        public DateTime Date { get; set; }
        public TimeSpan? Duration { get; set; } // Duration of the workout
        public string? Notes { get; set; }
        public bool? IsCompleted { get; set; } // Indicates if the workout was completed

    }
}
