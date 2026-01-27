using FluentValidation;
using MTM_Receiving_Application.Module_Receiving.Requests.Queries;

namespace MTM_Receiving_Application.Module_Receiving.Validators;

/// <summary>
/// Validator for ValidatePONumberQuery
/// Ensures PO number is provided
/// </summary>
public class Validator_Receiving_Shared_ValidateIf_POExists : AbstractValidator<QueryRequest_Receiving_Shared_Validate_PONumber>
{
    public Validator_Receiving_Shared_ValidateIf_POExists()
    {
        RuleFor(x => x.PONumber)
            .NotEmpty()
            .WithMessage("PO Number is required for validation");
    }
}
