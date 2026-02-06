using FluentValidation;
using MaoMao.Shared.DTO.User;

namespace MaoMao.API.DTO.User.Validation;

public class VerifyEmailRequestValidator : Validator<VerifyEmailRequest>
{
	public VerifyEmailRequestValidator()
	{
		RuleFor(x => x.Code)
			.NotEmpty().WithMessage("Cannot be empty.")
			.GreaterThanOrEqualTo(100000).WithMessage("Must be a 6 digit number.")
			.LessThanOrEqualTo(999999).WithMessage("Must be a 6 digit number.");
	}
}
