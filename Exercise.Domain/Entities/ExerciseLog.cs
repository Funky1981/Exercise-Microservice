using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise.Domain.Entities
{
    public class ExerciseLog
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public List<ExerciseLogEntry> ExercisesCompleted { get; set; } = new();
        public string? sets { get; set; }
        public string? reps { get; set; }
        
        public DateTime Date { get; set; }
        public TimeSpan? Duration { get; set; } // Duration of the workout
        public string? Notes { get; set; }
        public bool? IsCompleted { get; set; } // Indicates if the workout was completed

    }

    public class ExerciseLogEntry
    {
        public Guid ExerciseId { get; set; }
        public int Sets { get; set; }
        public int Reps { get; set; }
    }
}
