using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise.Domain.Entities
{
    public class WorkoutPlan
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public List<Workout> Workouts { get; set; } = new();
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; } 
        public string? Notes { get; set; }
        public bool? IsActive { get; set; }

    }
}
