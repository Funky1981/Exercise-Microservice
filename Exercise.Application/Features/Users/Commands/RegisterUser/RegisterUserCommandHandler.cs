using Exercise.Application.Abstractions.Repositories;
using Exercise.Domain.Entities;
using MediatR;

namespace Exercise.Application.Features.Users.Commands.RegisterUser
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existing is not null)
                throw new InvalidOperationException($"A user with email '{request.Email}' already exists.");

            var user = new User(Guid.NewGuid(), request.Name, request.Email);
            user.SetPassword(request.Password);

            if (!string.IsNullOrWhiteSpace(request.UserName))
                user.UpdateProfile(request.UserName, null, null);

            await _userRepository.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return user.Id;
        }
    }
}
