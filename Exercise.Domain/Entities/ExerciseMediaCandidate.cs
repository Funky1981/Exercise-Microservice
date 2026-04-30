using Exercise.Domain.Common;

namespace Exercise.Domain.Entities
{
    public static class ExerciseMediaCandidateReviewStatuses
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string AutoAccepted = "AutoAccepted";
    }

    public class ExerciseMediaCandidate
    {
        public Guid Id { get; private set; }
        public Guid ExerciseId { get; private set; }
        public string MediaUrl { get; private set; } = null!;
        public string? MediaKind { get; private set; }
        public string? ThumbnailUrl { get; private set; }
        public string? SourcePageUrl { get; private set; }
        public string SourceProvider { get; private set; } = null!;
        public string? SourcePayloadJson { get; private set; }
        public string? SourceTitle { get; private set; }
        public decimal MatchScore { get; private set; }
        public string ReviewStatus { get; private set; } = ExerciseMediaCandidateReviewStatuses.Pending;
        public string? ReviewNotes { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ReviewedAt { get; private set; }
        public bool IsSelected { get; private set; }

        public Exercise? Exercise { get; private set; }

        private ExerciseMediaCandidate() { }

        public ExerciseMediaCandidate(
            Guid id,
            Guid exerciseId,
            string mediaUrl,
            string? mediaKind,
            string? thumbnailUrl,
            string? sourcePageUrl,
            string sourceProvider,
            string? sourcePayloadJson,
            string? sourceTitle,
            decimal matchScore)
        {
            Guard.AgainstEmptyGuid(id, nameof(id));
            Guard.AgainstEmptyGuid(exerciseId, nameof(exerciseId));
            Guard.AgainstNullOrWhiteSpace(mediaUrl, nameof(mediaUrl));
            Guard.AgainstNullOrWhiteSpace(sourceProvider, nameof(sourceProvider));

            Id = id;
            ExerciseId = exerciseId;
            MediaUrl = mediaUrl;
            MediaKind = mediaKind;
            ThumbnailUrl = thumbnailUrl;
            SourcePageUrl = sourcePageUrl;
            SourceProvider = sourceProvider;
            SourcePayloadJson = sourcePayloadJson;
            SourceTitle = sourceTitle;
            MatchScore = ClampScore(matchScore);
            CreatedAt = DateTime.UtcNow;
        }

        public bool IsRejected => string.Equals(ReviewStatus, ExerciseMediaCandidateReviewStatuses.Rejected, StringComparison.OrdinalIgnoreCase);

        public void Refresh(
            string mediaUrl,
            string? mediaKind,
            string? thumbnailUrl,
            string? sourcePageUrl,
            string sourceProvider,
            string? sourcePayloadJson,
            string? sourceTitle,
            decimal matchScore)
        {
            Guard.AgainstNullOrWhiteSpace(mediaUrl, nameof(mediaUrl));
            Guard.AgainstNullOrWhiteSpace(sourceProvider, nameof(sourceProvider));

            MediaUrl = mediaUrl;
            MediaKind = mediaKind;
            ThumbnailUrl = thumbnailUrl;
            SourcePageUrl = sourcePageUrl;
            SourceProvider = sourceProvider;
            SourcePayloadJson = sourcePayloadJson;
            SourceTitle = sourceTitle;
            MatchScore = ClampScore(matchScore);
        }

        public void Approve(DateTime reviewedAtUtc, string? reviewNotes = null)
        {
            ReviewStatus = ExerciseMediaCandidateReviewStatuses.Approved;
            ReviewNotes = reviewNotes;
            ReviewedAt = reviewedAtUtc;
            IsSelected = true;
        }

        public void AutoAccept(DateTime reviewedAtUtc, string? reviewNotes = null)
        {
            ReviewStatus = ExerciseMediaCandidateReviewStatuses.AutoAccepted;
            ReviewNotes = reviewNotes;
            ReviewedAt = reviewedAtUtc;
            IsSelected = true;
        }

        public void Reject(DateTime reviewedAtUtc, string? reviewNotes = null)
        {
            ReviewStatus = ExerciseMediaCandidateReviewStatuses.Rejected;
            ReviewNotes = reviewNotes;
            ReviewedAt = reviewedAtUtc;
            IsSelected = false;
        }

        public void ClearSelection()
        {
            IsSelected = false;
        }

        private static decimal ClampScore(decimal score)
        {
            if (score < 0m)
            {
                return 0m;
            }

            if (score > 1m)
            {
                return 1m;
            }

            return score;
        }
    }
}