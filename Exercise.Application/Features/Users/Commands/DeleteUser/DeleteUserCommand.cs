using MediatR;

namespace Exercise.Application.Features.Users.Commands.DeleteUser
{
    public class DeleteUserCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public DeleteUserCommand(Guid userId) => UserId = userId;
    }
}
