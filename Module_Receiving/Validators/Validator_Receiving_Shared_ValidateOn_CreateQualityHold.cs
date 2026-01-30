using System;
using FluentValidation;
using MTM_Receiving_Application.Module_Receiving.Requests.Commands;

namespace MTM_Receiving_Application.Module_Receiving.Validators;

/// <summary>
/// Validator for CommandRequest_Receiving_Shared_Create_QualityHold
/// Ensures all required fields are present before creating quality hold record
/// User Requirement: Configurable part patterns (not hardcoded)
/// </summary>
public class Validator_Receiving_Shared_ValidateOn_CreateQualityHold : AbstractValidator<CommandRequest_Receiving_Shared_Create_QualityHold>
{
    public Validator_Receiving_Shared_ValidateOn_CreateQualityHold()
    {
        RuleFor(x => x.LineId)
            .NotEmpty()
            .WithMessage("Line ID is required")
            .Must(lineId => Guid.TryParse(lineId, out _))
            .WithMessage("Line ID must be a valid GUID");

        RuleFor(x => x.TransactionId)
            .NotEmpty()
            .WithMessage("Transaction ID is required")
            .Must(transactionId => Guid.TryParse(transactionId, out _))
            .WithMessage("Transaction ID must be a valid GUID");

        RuleFor(x => x.PartNumber)
            .NotEmpty()
            .WithMessage("Part Number is required")
            .MaximumLength(50)
            .WithMessage("Part Number cannot exceed 50 characters");

        RuleFor(x => x.PartPattern)
            .NotEmpty()
            .WithMessage("Part Pattern is required (configurable pattern that triggered quality hold)")
            .MaximumLength(100)
            .WithMessage("Part Pattern cannot exceed 100 characters");

        RuleFor(x => x.RestrictionType)
            .NotEmpty()
            .WithMessage("Restriction Type is required (e.g., 'Weight Sensitive', 'Quality Control')")
            .MaximumLength(50)
            .WithMessage("Restriction Type cannot exceed 50 characters");

        RuleFor(x => x.LoadNumber)
            .GreaterThan(0)
            .WithMessage("Load Number must be greater than zero");

        RuleFor(x => x.TotalWeight)
            .GreaterThan(0)
            .When(x => x.TotalWeight.HasValue)
            .WithMessage("Total Weight must be greater than zero when provided");

        RuleFor(x => x.UserAcknowledgedDate)
            .NotEmpty()
            .WithMessage("User Acknowledged Date is required (Step 1 acknowledgment)");

        RuleFor(x => x.UserAcknowledgedBy)
            .NotEmpty()
            .WithMessage("User Acknowledged By is required (username for Step 1)")
            .MaximumLength(100)
            .WithMessage("User Acknowledged By cannot exceed 100 characters");

        RuleFor(x => x.CreatedBy)
            .NotEmpty()
            .WithMessage("Created By is required (username for audit trail)")
            .MaximumLength(100)
            .WithMessage("Created By cannot exceed 100 characters");
    }
}
