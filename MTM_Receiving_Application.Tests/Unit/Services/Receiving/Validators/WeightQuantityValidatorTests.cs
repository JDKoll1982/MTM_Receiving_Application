using Moq;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.Models.Receiving.StepData;
using MTM_Receiving_Application.Services.Receiving.Validators;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Services.Receiving.Validators
{
    public class WeightQuantityValidatorTests
    {
        private readonly Mock<IService_ReceivingValidation> _mockValidationService;
        private readonly WeightQuantityValidator _validator;

        public WeightQuantityValidatorTests()
        {
            _mockValidationService = new Mock<IService_ReceivingValidation>();
            _validator = new WeightQuantityValidator(_mockValidationService.Object);
        }

        [Fact]
        public async void ValidateAsync_ShouldReturnFailure_WhenInputIsNull()
        {
            // Act
            var result = await _validator.ValidateAsync(null!);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Weight/Quantity data is required", result.ErrorMessage);
        }

        [Fact]
        public async void ValidateAsync_ShouldReturnSuccess_WhenAllLoadsHaveValidWeights()
        {
            // Arrange
            var input = new WeightQuantityData
            {
                Loads = new System.Collections.Generic.List<Model_ReceivingLoad>
                {
                    new Model_ReceivingLoad { LoadNumber = 1, WeightQuantity = 100 },
                    new Model_ReceivingLoad { LoadNumber = 2, WeightQuantity = 200 }
                }
            };

            _mockValidationService.Setup(s => s.ValidateWeightQuantity(It.IsAny<decimal>()))
                .Returns(new Service_ReceivingValidation.ValidationResult { IsValid = true });

            // Act
            var result = await _validator.ValidateAsync(input);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public async void ValidateAsync_ShouldReturnFailure_WhenAnyLoadHasInvalidWeight()
        {
            // Arrange
            var input = new WeightQuantityData
            {
                Loads = new System.Collections.Generic.List<Model_ReceivingLoad>
                {
                    new Model_ReceivingLoad { LoadNumber = 1, WeightQuantity = 100 },
                    new Model_ReceivingLoad { LoadNumber = 2, WeightQuantity = 0 },
                    new Model_ReceivingLoad { LoadNumber = 3, WeightQuantity = 150 }
                }
            };

            _mockValidationService.Setup(s => s.ValidateWeightQuantity(100))
                .Returns(new Service_ReceivingValidation.ValidationResult { IsValid = true });
            _mockValidationService.Setup(s => s.ValidateWeightQuantity(0))
                .Returns(new Service_ReceivingValidation.ValidationResult 
                { 
                    IsValid = false, 
                    Message = "Weight must be greater than zero" 
                });
            _mockValidationService.Setup(s => s.ValidateWeightQuantity(150))
                .Returns(new Service_ReceivingValidation.ValidationResult { IsValid = true });

            // Act
            var result = await _validator.ValidateAsync(input);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Load 2", result.ErrorMessage);
            Assert.Contains("Weight must be greater than zero", result.ErrorMessage);
        }

        [Fact]
        public async void ValidateAsync_ShouldReturnMultipleErrors_WhenMultipleLoadsHaveInvalidWeights()
        {
            // Arrange
            var input = new WeightQuantityData
            {
                Loads = new System.Collections.Generic.List<Model_ReceivingLoad>
                {
                    new Model_ReceivingLoad { LoadNumber = 1, WeightQuantity = 0 },
                    new Model_ReceivingLoad { LoadNumber = 2, WeightQuantity = -10 }
                }
            };

            _mockValidationService.Setup(s => s.ValidateWeightQuantity(0))
                .Returns(new Service_ReceivingValidation.ValidationResult 
                { 
                    IsValid = false, 
                    Message = "Weight must be greater than zero" 
                });
            _mockValidationService.Setup(s => s.ValidateWeightQuantity(-10))
                .Returns(new Service_ReceivingValidation.ValidationResult 
                { 
                    IsValid = false, 
                    Message = "Weight cannot be negative" 
                });

            // Act
            var result = await _validator.ValidateAsync(input);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(2, result.Errors.Count);
            Assert.Contains("Load 1", result.Errors[0]);
            Assert.Contains("Load 2", result.Errors[1]);
        }
    }
}
