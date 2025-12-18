using MTM_Receiving_Application.Contracts.Services.Validation;
using MTM_Receiving_Application.Models.Receiving.StepData;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Services.Receiving.Validators
{
    /// <summary>
    /// Validates PO number entry and part selection.
    /// Extracted from Service_ReceivingWorkflow switch statement (lines 106-123).
    /// </summary>
    public class PONumberValidator : IStepValidator<POEntryData>
    {
        public Task<ValidationResult> ValidateAsync(POEntryData input)
        {
            if (input == null)
            {
                return Task.FromResult(ValidationResult.Failure("PO Entry data is required."));
            }

            // If not a non-PO item, PO number is required
            if (string.IsNullOrEmpty(input.PONumber) && !input.IsNonPOItem)
            {
                return Task.FromResult(ValidationResult.Failure("PO Number is required."));
            }

            // Part selection is always required (whether PO or non-PO item)
            if (input.SelectedPart == null)
            {
                return Task.FromResult(ValidationResult.Failure("Part selection is required."));
            }

            return Task.FromResult(ValidationResult.Success());
        }
    }
}
