using FluentValidation;
using BookingDomain.Entities;

namespace BookingApplication.Validator;

public class UserValidator : AbstractValidator<Users> 
{
    public UserValidator()
    {
        RuleFor(user => user.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email address");
        
        RuleFor(user => user.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long");
        
        RuleFor(user => user.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MinimumLength(3).WithMessage("First name must be at least 3 characters long");
        
        RuleFor(user => user.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MinimumLength(3).WithMessage("Last name must be at least 3 characters long");
        
        RuleFor(user => user.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .MinimumLength(10).WithMessage("Phone number must be at least 10 characters long");

        RuleFor(user => user.ProfilePictureUrl)
            .NotEmpty().WithMessage("Profile picture is required")
            .When(user => !string.IsNullOrEmpty(user.ProfilePictureUrl));
        
        RuleFor(user => user.IsActive)
            .NotNull().WithMessage("Is active is required");
        
        RuleFor(user => user.CreatedAt)
            .NotNull().WithMessage("Created at is required")
            .Must(x => x != DateTime.MinValue).WithMessage("Created at must be a valid date");
        
        RuleFor(user => user.UpdatedAt)
            .NotNull().WithMessage("Updated at is required")
            .Must(x => x != DateTime.MinValue).WithMessage("Updated at must be a valid date");
    }
}