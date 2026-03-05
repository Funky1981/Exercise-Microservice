using Exercise.Application.Features.Workouts.Dtos;
using MediatR;

namespace Exercise.Application.Features.Workouts.Queries.GetWorkoutById
{
    public class GetWorkoutByIdQuery : IRequest<WorkoutDto?>
    {
        public Guid Id { get; set; }
        public GetWorkoutByIdQuery(Guid id) => Id = id;
    }
}
