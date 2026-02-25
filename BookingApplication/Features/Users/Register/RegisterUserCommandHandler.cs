using MediatR;
using BookingApplication.Abstractions.Contracts.Repositories;
using BCrypt.Net;
using FluentValidation;
using UserEntity = BookingDomain.Entities.Users;

namespace BookingApplication.Features.Users.Register;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IValidator<UserEntity> _validator;

    public RegisterUserCommandHandler(IUserRepository userRepository, IValidator<UserEntity> validator)
    {
        _userRepository = userRepository;
        _validator = validator;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        
        //check for unique email first
        var existingUser = await _userRepository.GetUserByEmail(request.CreateUserDto.Email, cancellationToken);
        if (existingUser != null)
        {
            throw new Exception("User with this email already exists");
        }
        
        var user = new UserEntity()
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