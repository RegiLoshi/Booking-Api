using MediatR;
using BookingDomain;
using BookingDomain.Repositories;
using UserEntity = BookingDomain.Entities.Users;
using BCrypt.Net;
using FluentValidation;

namespace BookingApplication.Features.Users.Register;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IRepository<UserEntity> _userRepository;
    private readonly IValidator<UserEntity> _validator;

    public RegisterUserCommandHandler(IRepository<UserEntity> userRepository, IValidator<UserEntity> validator)
    {
        _userRepository = userRepository;
        _validator = validator;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        
        //check for unique email first
        
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            FirstName = request.CreateUserDto.FirstName,
            LastName = request.CreateUserDto.LastName,
            Email = request.CreateUserDto.Email,
            Password = BCrypt.Net.BCrypt.EnhancedHashPassword(request.CreateUserDto.Password, 13),
            PhoneNumber = request.CreateUserDto.PhoneNumber,
            ProfilePictureUrl = request.CreateUserDto.ProfilePictureUrl ?? string.Empty,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _validator.ValidateAndThrowAsync(user, cancellationToken);

        await _userRepository.Add(user, cancellationToken);

        return user.Id;
    }
}