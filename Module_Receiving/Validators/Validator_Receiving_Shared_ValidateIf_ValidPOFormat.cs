using FluentValidation;
using MTM_Receiving_Application.Module_Receiving.Requests.Queries;

namespace MTM_Receiving_Application.Module_Receiving.Validators;

/// <summary>
/// Validator for GetReceivingLinesByPOQuery
/// Ensures PO number format is valid
/// </summary>
public class Validator_Receiving_Shared_ValidateIf_ValidPOFormat : AbstractValidator<QueryRequest_Receiving_Shared_Get_ReceivingLinesByPO>
{
    public Validator_Receiving_Shared_ValidateIf_ValidPOFormat()
    {
        RuleFor(x => x.PONumber)
            .NotEmpty()
            .WithMessage("PO Number is required")
            .Matches(@"^PO-\d{6}$")
            .WithMessage("Invalid PO Number format (must be PO- followed by 6 digits)");
    }
}
