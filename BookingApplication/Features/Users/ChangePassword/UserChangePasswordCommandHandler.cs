using BookingApplication.Abstractions.Contracts.Repositories;
using FluentValidation;
using MediatR;
using BCrypt.Net;
namespace BookingApplication.Features.Users.ChangePassword;

public class UserChangePasswordCommandHandler
    (IUserRepository _userRepository, IValidator<BookingDomain.Entities.Users> _validator)
    : IRequestHandler<UserChangePasswordCommand, Guid>
{
    public async Task<Guid> Handle(UserChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetUserById(request.userChangePasswordDto.Id, cancellationToken);
        
        if (existingUser == null)
        {
            throw new Exception("User does not exist");
        }
        
        if (!BCrypt.Net.BCrypt.EnhancedVerify(request.userChangePasswordDto.OldPassword, existingUser.Password))
        {
            throw new Exception("Invalid password");
        }
        
        existingUser.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(request.userChangePasswordDto.NewPassword, 13);
        
        await _validator.ValidateAndThrowAsync(existingUser, cancellationToken);

        await _userRepository.UpdateUser(existingUser, cancellationToken);

        return existingUser.Id;
    }
}