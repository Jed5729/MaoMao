using FluentValidation;

namespace MaoMao.API.DTO.Auth.Validation;

public class LoginRequestValidator : Validator<LoginRequest>
{
	public LoginRequestValidator()
	{
		RuleFor(x => x.Username)
			.NotEmpty().WithMessage("Username cannot be empty.")
			.MinimumLength(3).WithMessage("Username must be at least 3 characters.")
			.MaximumLength(16).WithMessage("Username must not exceeed 16 characters.");
		RuleFor(x => x.PreHashedPassword)
			.NotEmpty().WithMessage("Password hash must be provided.")
			.Matches("^[a-fA-F0-9]{64}$").WithMessage("Password hash must be a valid SHA-256 hex string.");
	}
}
