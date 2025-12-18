using MTM_Receiving_Application.Contracts.Services.Validation;
using MTM_Receiving_Application.Models.Receiving.StepData;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Services.Receiving.Validators
{
    /// <summary>
    /// Validates number of loads to create.
    /// Extracted from Service_ReceivingWorkflow switch statement (lines 125-131).
    /// </summary>
    public class LoadCountValidator : IStepValidator<LoadEntryData>
    {
        public Task<ValidationResult> ValidateAsync(LoadEntryData input)
        {
            if (input == null)
            {
                return Task.FromResult(ValidationResult.Failure("Load Entry data is required."));
            }

            if (input.NumberOfLoads < 1)
            {
                return Task.FromResult(ValidationResult.Failure("Number of loads must be at least 1."));
            }

            return Task.FromResult(ValidationResult.Success());
        }
    }
}
