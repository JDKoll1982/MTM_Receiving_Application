using MTM_Receiving_Application.Contracts.Services.Validation;
using MTM_Receiving_Application.Models.Receiving.StepData;
using System.Collections.Generic;
using System.Threading.Tasks;
using IService_ReceivingValidation = MTM_Receiving_Application.Contracts.Services.IService_ReceivingValidation;

namespace MTM_Receiving_Application.Services.Receiving.Validators
{
    /// <summary>
    /// Validates weight/quantity values for all loads.
    /// Extracted from Service_ReceivingWorkflow switch statement (lines 135-149).
    /// </summary>
    public class WeightQuantityValidator : IStepValidator<WeightQuantityData>
    {
        private readonly IService_ReceivingValidation _validationService;

        public WeightQuantityValidator(IService_ReceivingValidation validationService)
        {
            _validationService = validationService ?? throw new System.ArgumentNullException(nameof(validationService));
        }

        public Task<ValidationResult> ValidateAsync(WeightQuantityData input)
        {
            if (input == null)
            {
                return Task.FromResult(ValidationResult.Failure("Weight/Quantity data is required."));
            }

            var errors = new List<string>();

            foreach (var load in input.Loads)
            {
                var result = _validationService.ValidateWeightQuantity(load.WeightQuantity);
                if (!result.IsValid)
                {
                    errors.Add($"Load {load.LoadNumber}: {result.Message}");
                }
            }

            if (errors.Count > 0)
            {
                return Task.FromResult(ValidationResult.Failure(errors));
            }

            return Task.FromResult(ValidationResult.Success());
        }
    }
}
