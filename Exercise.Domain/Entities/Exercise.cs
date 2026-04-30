using System;
using Exercise.Domain.Common;

namespace Exercise.Domain.Entities
{
    public class Exercise
    {
        public Guid Id { get; private set; }
        public string? ExternalId { get; private set; }
        public string? SourceProvider { get; private set; }
        public string Name { get; private set; } = null!;
        public string BodyPart { get; private set; } = null!;
        public string? Equipment { get; private set; }
        public string TargetMuscle { get; private set; } = null!;
        public string? GifUrl { get; private set; }
        public string? MediaUrl { get; private set; }
        public string? MediaKind { get; private set; }
        public string? MediaThumbnailUrl { get; private set; }
        public string? MediaSourcePageUrl { get; private set; }
        public string? MediaSourceProvider { get; private set; }
        public string? MediaSourcePayloadJson { get; private set; }
        public string? SecondaryMusclesJson { get; private set; }
        public string? InstructionsJson { get; private set; }
        public string? SourcePayloadJson { get; private set; }
        public string? Description { get; private set; }
        public string? Difficulty { get; private set; }
        public string? Category { get; private set; }
        public ICollection<ExerciseMediaCandidate> MediaCandidates { get; private set; } = [];

        private Exercise() { } // For EF Core

        public Exercise(Guid id, string name, string bodyPart, string targetMuscle,
            string? equipment = null, string? gifUrl = null, string? description = null, string? difficulty = null,
            string? externalId = null, string? sourceProvider = null, string? secondaryMusclesJson = null,
            string? instructionsJson = null, string? sourcePayloadJson = null, string? category = null,
            string? mediaUrl = null, string? mediaKind = null, string? mediaThumbnailUrl = null,
            string? mediaSourcePageUrl = null, string? mediaSourceProvider = null, string? mediaSourcePayloadJson = null)
        {
            Guard.AgainstEmptyGuid(id, nameof(id));
            Guard.AgainstNullOrWhiteSpace(name, nameof(name));
            Guard.AgainstNullOrWhiteSpace(bodyPart, nameof(bodyPart));
            Guard.AgainstNullOrWhiteSpace(targetMuscle, nameof(targetMuscle));

            Id = id;
            ExternalId = externalId;
            SourceProvider = sourceProvider;
            Name = name;
            BodyPart = bodyPart;
            TargetMuscle = targetMuscle;
            Equipment = equipment;
            GifUrl = gifUrl;
            MediaUrl = mediaUrl;
            MediaKind = mediaKind;
            MediaThumbnailUrl = mediaThumbnailUrl;
            MediaSourcePageUrl = mediaSourcePageUrl;
            MediaSourceProvider = mediaSourceProvider;
            MediaSourcePayloadJson = mediaSourcePayloadJson;
            SecondaryMusclesJson = secondaryMusclesJson;
            InstructionsJson = instructionsJson;
            SourcePayloadJson = sourcePayloadJson;
            Description = description;
            Difficulty = difficulty;
            Category = category;
        }

        public void UpdateDescription(string? description)
        {
            Description = description;
        }

        public void UpdateGifUrl(string? gifUrl)
        {
            GifUrl = gifUrl;
        }

        public void Update(string name, string bodyPart, string targetMuscle,
            string? equipment = null, string? gifUrl = null, string? description = null, string? difficulty = null,
            string? externalId = null, string? sourceProvider = null, string? secondaryMusclesJson = null,
            string? instructionsJson = null, string? sourcePayloadJson = null, string? category = null,
            string? mediaUrl = null, string? mediaKind = null, string? mediaThumbnailUrl = null,
            string? mediaSourcePageUrl = null, string? mediaSourceProvider = null, string? mediaSourcePayloadJson = null)
        {
            Guard.AgainstNullOrWhiteSpace(name, nameof(name));
            Guard.AgainstNullOrWhiteSpace(bodyPart, nameof(bodyPart));
            Guard.AgainstNullOrWhiteSpace(targetMuscle, nameof(targetMuscle));

            Name = name;
            BodyPart = bodyPart;
            TargetMuscle = targetMuscle;
            Equipment = equipment;
            GifUrl = gifUrl;
            MediaUrl = mediaUrl;
            MediaKind = mediaKind;
            MediaThumbnailUrl = mediaThumbnailUrl;
            MediaSourcePageUrl = mediaSourcePageUrl;
            MediaSourceProvider = mediaSourceProvider;
            MediaSourcePayloadJson = mediaSourcePayloadJson;
            ExternalId = externalId;
            SourceProvider = sourceProvider;
            SecondaryMusclesJson = secondaryMusclesJson;
            InstructionsJson = instructionsJson;
            SourcePayloadJson = sourcePayloadJson;
            Description = description;
            Difficulty = difficulty;
            Category = category;
        }

        public void ApplyExternalData(
            string name,
            string bodyPart,
            string targetMuscle,
            string? equipment,
            string? gifUrl,
            string? externalId,
            string? sourceProvider,
            string? secondaryMusclesJson,
            string? instructionsJson,
            string? sourcePayloadJson,
            string? description,
            string? difficulty,
            string? category,
            string? mediaUrl,
            string? mediaKind,
            string? mediaThumbnailUrl,
            string? mediaSourcePageUrl,
            string? mediaSourceProvider,
            string? mediaSourcePayloadJson)
        {
            Update(
                name,
                bodyPart,
                targetMuscle,
                equipment,
                gifUrl,
                description,
                difficulty,
                externalId,
                sourceProvider,
                secondaryMusclesJson,
                instructionsJson,
                sourcePayloadJson,
                category,
                mediaUrl,
                mediaKind,
                mediaThumbnailUrl,
                mediaSourcePageUrl,
                mediaSourceProvider,
                mediaSourcePayloadJson);
        }

        public void ApplyMediaData(
            string mediaUrl,
            string? mediaKind,
            string? mediaThumbnailUrl,
            string? mediaSourcePageUrl,
            string? mediaSourceProvider,
            string? mediaSourcePayloadJson)
        {
            MediaUrl = mediaUrl;
            MediaKind = mediaKind;
            MediaThumbnailUrl = mediaThumbnailUrl;
            MediaSourcePageUrl = mediaSourcePageUrl;
            MediaSourceProvider = mediaSourceProvider;
            MediaSourcePayloadJson = mediaSourcePayloadJson;

            if (string.IsNullOrWhiteSpace(GifUrl) && string.Equals(mediaKind, "image/gif", StringComparison.OrdinalIgnoreCase))
            {
                GifUrl = mediaUrl;
            }
        }

        public void ClearMediaData()
        {
            MediaUrl = null;
            MediaKind = null;
            MediaThumbnailUrl = null;
            MediaSourcePageUrl = null;
            MediaSourceProvider = null;
            MediaSourcePayloadJson = null;
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
