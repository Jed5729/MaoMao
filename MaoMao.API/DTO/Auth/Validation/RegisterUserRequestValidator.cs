using FluentValidation;
using MaoMao.Shared.DTO.Auth;

namespace MaoMao.API.DTO.Auth.Validation;

public class RegisterUserRequestValidator : Validator<RegisterUserRequest>
{
	public RegisterUserRequestValidator()
	{
		RuleFor(x => x.Username)
			.NotEmpty().WithMessage("Username cannot be empty.")
			.MinimumLength(3).WithMessage("Username must be at least 3 characters.")
			.MaximumLength(16).WithMessage("Username must not exceeed 16 characters.");

		RuleFor(x => x.Email)
			.NotEmpty().WithMessage("An email address is required.")
			.EmailAddress().WithMessage("Must be a valid email address");

		RuleFor(x => x.PreHashedPassword)
			.NotEmpty().WithMessage("Password hash must be provided.")
			.Matches("^[a-fA-F0-9]{64}$").WithMessage("Password hash must be a valid SHA-256 hex string.");
	}
}
