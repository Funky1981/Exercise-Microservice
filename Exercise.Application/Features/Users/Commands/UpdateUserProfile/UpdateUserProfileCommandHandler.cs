using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
using Exercise.Domain.ValueObjects;
using MediatR;

namespace Exercise.Application.Features.Users.Commands.UpdateUserProfile
{
    public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateUserProfileCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
                throw new NotFoundException(nameof(user), request.UserId);

            var height = request.HeightCm.HasValue ? Height.FromCentimeters(request.HeightCm.Value) : null;
            var weight = request.WeightKg.HasValue ? Weight.FromKilograms(request.WeightKg.Value) : null;

            user.UpdateProfile(request.UserName, height, weight);

            await _userRepository.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
