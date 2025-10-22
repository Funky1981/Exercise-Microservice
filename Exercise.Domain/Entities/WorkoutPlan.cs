using System;
using System.Collections.Generic;
using System.Linq;
using Exercise.Domain.Common;

namespace Exercise.Domain.Entities
{
    public class WorkoutPlan
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string? Name { get; private set; }
        
        private readonly List<Workout> _workouts = new();
        public IReadOnlyList<Workout> Workouts => _workouts.AsReadOnly();
        
        public DateTime StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public string? Notes { get; private set; }
        public bool IsActive { get; private set; }

        private WorkoutPlan() { } // For EF Core

        public WorkoutPlan(Guid id, Guid userId, string? name, DateTime startDate, DateTime? endDate = null)
        {
            Guard.AgainstEmptyGuid(id, nameof(id));
            Guard.AgainstEmptyGuid(userId, nameof(userId));
            Guard.AgainstInvalidDateRange(startDate, endDate, nameof(endDate));

            Id = id;
            UserId = userId;
            Name = name;
            StartDate = startDate;
            EndDate = endDate;
            IsActive = false;
        }

        public void AddWorkout(Workout workout)
        {
            Guard.AgainstNull(workout, nameof(workout));
            
            if (workout.UserId != UserId)
                throw new InvalidOperationException("Cannot add workout for different user.");
            
            if (_workouts.Any(w => w.Id == workout.Id))
                return; // Already exists
                
            _workouts.Add(workout);
        }

        public void RemoveWorkout(Guid workoutId)
        {
            Guard.AgainstEmptyGuid(workoutId, nameof(workoutId));
                
            var workoutToRemove = _workouts.FirstOrDefault(w => w.Id == workoutId);
            if (workoutToRemove != null)
            {
                _workouts.Remove(workoutToRemove);
            }
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void UpdateNotes(string? notes)
        {
            Notes = notes;
        }
    }
}
