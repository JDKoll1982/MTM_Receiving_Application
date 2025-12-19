using MTM_Receiving_Application.Contracts.Services.Validation;
using MTM_Receiving_Application.Models.Receiving.StepData;
using System.Collections.Generic;
using System.Threading.Tasks;
using IService_ReceivingValidation = MTM_Receiving_Application.Contracts.Services.IService_ReceivingValidation;

namespace MTM_Receiving_Application.Services.Receiving.Validators
{
    /// <summary>
    /// Validates package type and count for all loads.
    /// Extracted from Service_ReceivingWorkflow switch statement (lines 167-185).
    /// </summary>
    public class PackageTypeValidator : IStepValidator<PackageTypeData>
    {
        private readonly IService_ReceivingValidation _validationService;

        public PackageTypeValidator(IService_ReceivingValidation validationService)
        {
            _validationService = validationService ?? throw new System.ArgumentNullException(nameof(validationService));
        }

        public Task<ValidationResult> ValidateAsync(PackageTypeData input)
        {
            if (input == null)
            {
                return Task.FromResult(ValidationResult.Failure("Package Type data is required."));
            }

            var errors = new List<string>();

            foreach (var load in input.Loads)
            {
                // Validate package count
                var countResult = _validationService.ValidatePackageCount(load.PackagesPerLoad);
                if (!countResult.IsValid)
                {
                    errors.Add($"Load {load.LoadNumber}: {countResult.Message}");
                }

                // Validate package type name
                if (string.IsNullOrWhiteSpace(load.PackageTypeName))
                {
                    errors.Add($"Load {load.LoadNumber}: Package Type is required.");
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
