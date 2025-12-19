using MTM_Receiving_Application.Contracts.Services.Validation;
using MTM_Receiving_Application.Models.Receiving.StepData;
using System.Collections.Generic;
using System.Threading.Tasks;
using IService_ReceivingValidation = MTM_Receiving_Application.Contracts.Services.IService_ReceivingValidation;

namespace MTM_Receiving_Application.Services.Receiving.Validators
{
    /// <summary>
    /// Validates heat/lot numbers for all loads.
    /// Extracted from Service_ReceivingWorkflow switch statement (lines 151-165).
    /// </summary>
    public class HeatLotValidator : IStepValidator<HeatLotData>
    {
        private readonly IService_ReceivingValidation _validationService;

        public HeatLotValidator(IService_ReceivingValidation validationService)
        {
            _validationService = validationService ?? throw new System.ArgumentNullException(nameof(validationService));
        }

        public Task<ValidationResult> ValidateAsync(HeatLotData input)
        {
            if (input == null)
            {
                return Task.FromResult(ValidationResult.Failure("Heat/Lot data is required."));
            }

            var errors = new List<string>();

            foreach (var load in input.Loads)
            {
                var result = _validationService.ValidateHeatLotNumber(load.HeatLotNumber);
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
