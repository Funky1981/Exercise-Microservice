using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Exercises.Dtos;
using MediatR;

namespace Exercise.Application.Features.Exercises.Queries.GetPendingExerciseMediaCandidates
{
    public sealed class GetPendingExerciseMediaCandidatesQueryHandler : IRequestHandler<GetPendingExerciseMediaCandidatesQuery, IReadOnlyList<ExerciseMediaReviewQueueItemDto>>
    {
        private readonly IExerciseMediaCandidateRepository _candidateRepository;

        public GetPendingExerciseMediaCandidatesQueryHandler(IExerciseMediaCandidateRepository candidateRepository)
        {
            _candidateRepository = candidateRepository;
        }

        public async Task<IReadOnlyList<ExerciseMediaReviewQueueItemDto>> Handle(GetPendingExerciseMediaCandidatesQuery request, CancellationToken cancellationToken)
        {
            var candidates = await _candidateRepository.GetPendingAsync(request.Limit, cancellationToken);

            return candidates
                .Select(candidate => new ExerciseMediaReviewQueueItemDto
                {
                    Id = candidate.Id,
                    ExerciseId = candidate.ExerciseId,
                    ExerciseName = candidate.Exercise?.Name ?? string.Empty,
                    ExerciseBodyPart = candidate.Exercise?.BodyPart ?? string.Empty,
                    ExerciseTargetMuscle = candidate.Exercise?.TargetMuscle ?? string.Empty,
                    ExerciseEquipment = candidate.Exercise?.Equipment,
                    MediaUrl = candidate.MediaUrl,
                    MediaKind = candidate.MediaKind,
                    ThumbnailUrl = candidate.ThumbnailUrl,
                    SourcePageUrl = candidate.SourcePageUrl,
                    SourceProvider = candidate.SourceProvider,
                    SourceTitle = candidate.SourceTitle,
                    MatchScore = candidate.MatchScore,
                    ReviewStatus = candidate.ReviewStatus,
                    IsSelected = candidate.IsSelected,
                    CreatedAt = candidate.CreatedAt,
                })
                .ToList();
        }
    }
}