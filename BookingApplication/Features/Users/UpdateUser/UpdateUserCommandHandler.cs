using BookingApplication.Abstractions.Contracts.Repositories;
using FluentValidation;
using MediatR;

namespace BookingApplication.Features.Users.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IValidator<BookingDomain.Entities.Users> _validator;
    
    public UpdateUserCommandHandler(IUserRepository userRepository, IValidator<BookingDomain.Entities.Users> validator)
    {
        _userRepository = userRepository;
        _validator =  validator;
    }

    public async Task<Guid> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetUserById(request.UpdateUserDto.Id, cancellationToken);
        
        if (existingUser == null)
        {
            throw new Exception("User does not exist");
        }

        existingUser.FirstName = request.UpdateUserDto.FirstName;
        existingUser.LastName = request.UpdateUserDto.LastName;
        existingUser.PhoneNumber = request.UpdateUserDto.PhoneNumber;
        existingUser.ProfilePictureUrl = request.UpdateUserDto.ProfilePictureUrl ?? string.Empty;
        existingUser.UpdatedAt = DateTime.UtcNow;
        
        await _validator.ValidateAndThrowAsync(existingUser, cancellationToken);

        await _userRepository.UpdateUser(existingUser, cancellationToken);

        return existingUser.Id;
    }
}