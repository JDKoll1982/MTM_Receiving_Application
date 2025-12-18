using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Contracts.Services.Validation;
using MTM_Receiving_Application.Models.Receiving;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Services.Receiving.Validators
{
    /// <summary>
    /// Validates complete session before saving to CSV and database.
    /// Uses existing IService_ReceivingValidation.ValidateSession() method.
    /// </summary>
    public class SessionValidator : IStepValidator<List<Model_ReceivingLoad>>
    {
        private readonly IService_ReceivingValidation _validationService;

        public SessionValidator(IService_ReceivingValidation validationService)
        {
            _validationService = validationService ?? throw new System.ArgumentNullException(nameof(validationService));
        }

        public Task<ValidationResult> ValidateAsync(List<Model_ReceivingLoad> loads)
        {
            if (loads == null || loads.Count == 0)
            {
                return Task.FromResult(ValidationResult.Failure("Session must contain at least one load."));
            }

            var validationResult = _validationService.ValidateSession(loads);

            if (!validationResult.IsValid)
            {
                return Task.FromResult(ValidationResult.Failure(validationResult.Errors));
            }

            return Task.FromResult(ValidationResult.Success());
        }
    }
}
