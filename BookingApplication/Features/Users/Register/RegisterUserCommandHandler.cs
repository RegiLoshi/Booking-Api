using MediatR;
using BookingApplication.Abstractions.Contracts.Repositories;
using BookingApplication.Abstractions.Contracts.Email;
using BCrypt.Net;
using FluentValidation;
using Microsoft.Extensions.Logging;
using UserEntity = BookingDomain.Entities.Users;

namespace BookingApplication.Features.Users.Register;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IValidator<UserEntity> _validator;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IValidator<UserEntity> validator,
        IEmailSender emailSender,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _validator = validator;
        _emailSender = emailSender;
        _logger = logger;
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

        try
        {
            var plainText = $"Hi {user.FirstName}, welcome to BookingApi!";
            var html = $"""
                        <!doctype html>
                        <html>
                          <body style="font-family: Arial, Helvetica, sans-serif; color:#111; line-height:1.4; margin:0; padding:0;">
                            <div style="max-width:640px; margin:0 auto; padding:24px;">
                              <h2 style="margin:0 0 16px 0;">Welcome to BookingApi</h2>
                              <p style="margin:0 0 16px 0;">Hi {System.Net.WebUtility.HtmlEncode(user.FirstName)},</p>
                              <p style="margin:0 0 16px 0;">Thanks for registering. You can now log in and start booking properties.</p>
                              <hr style="border:0; border-top:1px solid #eee; margin:20px 0;"/>
                              <p style="margin:0; font-size:12px; color:#666;">BookingApi</p>
                            </div>
                          </body>
                        </html>
                        """;

            await _emailSender.SendAsync(
                toEmail: user.Email,
                subject: "Welcome to BookingApi",
                plainTextContent: plainText,
                htmlContent: html,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send welcome email to {Email}", user.Email);
        }

        return user.Id;
    }
}