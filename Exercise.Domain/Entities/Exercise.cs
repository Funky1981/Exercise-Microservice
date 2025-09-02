using System;
using Exercise.Domain.Common;

namespace Exercise.Domain.Entities
{
    public class Exercise
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string BodyPart { get; private set; }
        public string? Equipment { get; private set; }
        public string TargetMuscle { get; private set; }
        public string? GifUrl { get; private set; }
        public string? Description { get; private set; }
        public string? Difficulty { get; private set; }

        private Exercise() { } // For EF Core

        public Exercise(Guid id, string name, string bodyPart, string targetMuscle, 
            string? equipment = null, string? gifUrl = null, string? description = null, string? difficulty = null)
        {
            Guard.AgainstEmptyGuid(id, nameof(id));
            Guard.AgainstNullOrWhiteSpace(name, nameof(name));
            Guard.AgainstNullOrWhiteSpace(bodyPart, nameof(bodyPart));
            Guard.AgainstNullOrWhiteSpace(targetMuscle, nameof(targetMuscle));

            Id = id;
            Name = name;
            BodyPart = bodyPart;
            TargetMuscle = targetMuscle;
            Equipment = equipment;
            GifUrl = gifUrl;
            Description = description;
            Difficulty = difficulty;
        }

        public void UpdateDescription(string? description)
        {
            Description = description;
        }

        public void UpdateGifUrl(string? gifUrl)
        {
            GifUrl = gifUrl;
        }

        public bool RequiresEquipment()
        {
            return !string.IsNullOrWhiteSpace(Equipment);
        }

        public override string ToString()
        {
            return $"{Name} ({BodyPart} - {TargetMuscle})";
        }
    }
}
