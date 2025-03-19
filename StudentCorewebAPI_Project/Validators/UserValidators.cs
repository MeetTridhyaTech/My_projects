using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StudentCorewebAPI_Project.Data;
using StudentCorewebAPI_Project.DTOs; // Ensure you use the correct namespace

namespace StudentCorewebAPI_Project.Validators
{
    public class UserValidators : AbstractValidator<AddUserDto>
    {
        private readonly ApplicationDbContext _context;
        public UserValidators(ApplicationDbContext context)
        {
            _context = context;

            RuleFor(s => s.FirstName)
                .NotEmpty().WithMessage("First Name is required")
                .MaximumLength(50).WithMessage("First Name cannot exceed 50 characters");

            RuleFor(s => s.LastName)
                .NotEmpty().WithMessage("Last Name is required");

            RuleFor(s => s.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(s => s.Mobile.ToString())
                .NotEmpty().WithMessage("Mobile number is required")
                .Matches(@"^\d{10}$").WithMessage("Mobile number must be exactly 10 digits");
                //.WithMessage("Mobile number is already in use.");

        }




    }
}

