using FluentValidation;
using StudentCorewebAPI_Project.Models;

namespace StudentCorewebAPI_Project.Validators
{
    public class UpdateUserValidators : AbstractValidator<UpdateUser> // Apply validation to UpdateStudent
    {
        public UpdateUserValidators()
        {
            RuleFor(s => s.FirstName)
                .NotEmpty().WithMessage("First Name is required")
                .MaximumLength(50).WithMessage("First Name cannot exceed 50 characters");

            RuleFor(s => s.LastName)
                .NotEmpty().WithMessage("Last Name is required");

            RuleFor(s => s.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(s => s.Mobile)
                .NotEmpty().WithMessage("Mobile number is required")
                .InclusiveBetween(1000000000, 9999999999).WithMessage("Mobile number must be exactly 10 digits");
        }
    }
}
