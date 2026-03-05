using Exercise.Application.Features.Users.Dtos;
using MediatR;

namespace Exercise.Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQuery : IRequest<UserDto?>
    {
        public Guid Id { get; set; }

        public GetUserByIdQuery(Guid id) => Id = id;
    }
}
