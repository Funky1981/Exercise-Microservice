using AutoMapper;
using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Models;
using Exercise.Application.Exercises.Dtos;
using Exercise.Application.Features.Exercises.Support;
using MediatR;

namespace Exercise.Application.Features.Exercises.Queries.GetAllExercises
{
    /// <summary>
    /// Handler for processing GetAllExercisesQuery requests
    /// </summary>
    public class GetAllExercisesQueryHandler : IRequestHandler<GetAllExercisesQuery, PagedResult<ExerciseDto>>
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IMapper _mapper;

        public GetAllExercisesQueryHandler(IExerciseRepository exerciseRepository, IMapper mapper)
        {
            _exerciseRepository = exerciseRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<ExerciseDto>> Handle(GetAllExercisesQuery request, CancellationToken cancellationToken)
        {
            var skip = (request.PageNumber - 1) * request.PageSize;
            var region = request.Region?.Trim().ToLowerInvariant();
            var otherRegionOnly = region == "other";
            var regionBodyParts = string.IsNullOrWhiteSpace(region)
                ? null
                : ExerciseRegionCatalog.GetBodyPartsForRegion(region);
            var (exercises, totalCount) = await _exerciseRepository.GetPagedAsync(
                skip,
                request.PageSize,
                request.Search,
                request.BodyPart,
                request.Equipment,
                regionBodyParts,
                otherRegionOnly,
                cancellationToken);

            var dtos = _mapper.Map<List<ExerciseDto>>(exercises);
            return new PagedResult<ExerciseDto>(dtos, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
