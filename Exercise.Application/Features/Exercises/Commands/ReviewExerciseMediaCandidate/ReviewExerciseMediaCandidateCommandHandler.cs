using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common;
using Exercise.Application.Common.Exceptions;
using MediatR;

namespace Exercise.Application.Features.Exercises.Commands.ReviewExerciseMediaCandidate
{
    public sealed class ReviewExerciseMediaCandidateCommandHandler : IRequestHandler<ReviewExerciseMediaCandidateCommand>
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IExerciseMediaCandidateRepository _candidateRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ReviewExerciseMediaCandidateCommandHandler(
            IExerciseRepository exerciseRepository,
            IExerciseMediaCandidateRepository candidateRepository,
            IUnitOfWork unitOfWork)
        {
            _exerciseRepository = exerciseRepository;
            _candidateRepository = candidateRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(ReviewExerciseMediaCandidateCommand request, CancellationToken cancellationToken)
        {
            var exercise = await _exerciseRepository.GetByIdForUpdateAsync(request.ExerciseId, cancellationToken);
            if (exercise is null)
            {
                throw new NotFoundException(nameof(Exercise.Domain.Entities.Exercise), request.ExerciseId);
            }

            var candidate = await _candidateRepository.GetByIdForUpdateAsync(request.CandidateId, cancellationToken);
            if (candidate is null || candidate.ExerciseId != exercise.Id)
            {
                throw new NotFoundException(nameof(Exercise.Domain.Entities.ExerciseMediaCandidate), request.CandidateId);
            }

            var allCandidates = (await _candidateRepository.GetByExerciseIdsForUpdateAsync([exercise.Id], cancellationToken)).ToList();
            var now = DateTime.UtcNow;

            if (request.Approve)
            {
                if (!ExerciseMediaPolicy.IsSupportedExampleMedia(candidate.MediaKind, candidate.MediaUrl))
                {
                    throw new InvalidOperationException("Only video exercise examples can be approved.");
                }

                foreach (var other in allCandidates.Where(other => other.Id != candidate.Id))
                {
                    other.ClearSelection();
                }

                candidate.Approve(now, request.Notes);
                exercise.ApplyMediaData(
                    candidate.MediaUrl,
                    candidate.MediaKind,
                    candidate.ThumbnailUrl,
                    candidate.SourcePageUrl,
                    candidate.SourceProvider,
                    candidate.SourcePayloadJson);
            }
            else
            {
                var wasSelected = candidate.IsSelected;
                candidate.Reject(now, request.Notes);

                if (wasSelected && string.Equals(exercise.MediaUrl, candidate.MediaUrl, StringComparison.OrdinalIgnoreCase))
                {
                    var fallback = allCandidates
                        .Where(other => other.Id != candidate.Id
                            && !other.IsRejected
                            && ExerciseMediaPolicy.IsSupportedExampleMedia(other.MediaKind, other.MediaUrl))
                        .OrderByDescending(other => other.IsSelected)
                        .ThenByDescending(other => other.MatchScore)
                        .FirstOrDefault();

                    if (fallback is null)
                    {
                        exercise.ClearMediaData();
                    }
                    else
                    {
                        foreach (var other in allCandidates.Where(other => other.Id != fallback.Id))
                        {
                            other.ClearSelection();
                        }

                        fallback.Approve(now, "Selected automatically after rejecting the previous media candidate.");
                        exercise.ApplyMediaData(
                            fallback.MediaUrl,
                            fallback.MediaKind,
                            fallback.ThumbnailUrl,
                            fallback.SourcePageUrl,
                            fallback.SourceProvider,
                            fallback.SourcePayloadJson);
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}