using Moq;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.Models.Receiving.StepData;
using MTM_Receiving_Application.Services.Receiving.Validators;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Services.Receiving.Validators
{
    public class HeatLotValidatorTests
    {
        private readonly Mock<IService_ReceivingValidation> _mockValidationService;
        private readonly HeatLotValidator _validator;

        public HeatLotValidatorTests()
        {
            _mockValidationService = new Mock<IService_ReceivingValidation>();
            _validator = new HeatLotValidator(_mockValidationService.Object);
        }

        [Fact]
        public async void ValidateAsync_ShouldReturnFailure_WhenInputIsNull()
        {
            // Act
            var result = await _validator.ValidateAsync(null!);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Heat/Lot data is required", result.ErrorMessage);
        }

        [Fact]
        public async void ValidateAsync_ShouldReturnSuccess_WhenAllLoadsHaveValidHeatNumbers()
        {
            // Arrange
            var input = new HeatLotData
            {
                Loads = new System.Collections.Generic.List<Model_ReceivingLoad>
                {
                    new Model_ReceivingLoad { LoadNumber = 1, HeatLotNumber = "HEAT-001" },
                    new Model_ReceivingLoad { LoadNumber = 2, HeatLotNumber = "HEAT-002" }
                }
            };

            _mockValidationService.Setup(s => s.ValidateHeatLotNumber(It.IsAny<string>()))
                .Returns(new Service_ReceivingValidation.ValidationResult { IsValid = true });

            // Act
            var result = await _validator.ValidateAsync(input);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public async void ValidateAsync_ShouldReturnFailure_WhenAnyLoadHasInvalidHeatNumber()
        {
            // Arrange
            var input = new HeatLotData
            {
                Loads = new System.Collections.Generic.List<Model_ReceivingLoad>
                {
                    new Model_ReceivingLoad { LoadNumber = 1, HeatLotNumber = "HEAT-001" },
                    new Model_ReceivingLoad { LoadNumber = 2, HeatLotNumber = "" },
                    new Model_ReceivingLoad { LoadNumber = 3, HeatLotNumber = "HEAT-003" }
                }
            };

            _mockValidationService.Setup(s => s.ValidateHeatLotNumber("HEAT-001"))
                .Returns(new Service_ReceivingValidation.ValidationResult { IsValid = true });
            _mockValidationService.Setup(s => s.ValidateHeatLotNumber(""))
                .Returns(new Service_ReceivingValidation.ValidationResult 
                { 
                    IsValid = false, 
                    Message = "Heat/Lot number is required" 
                });
            _mockValidationService.Setup(s => s.ValidateHeatLotNumber("HEAT-003"))
                .Returns(new Service_ReceivingValidation.ValidationResult { IsValid = true });

            // Act
            var result = await _validator.ValidateAsync(input);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Load 2", result.ErrorMessage);
            Assert.Contains("Heat/Lot number is required", result.ErrorMessage);
        }

        [Fact]
        public async void ValidateAsync_ShouldReturnMultipleErrors_WhenMultipleLoadsHaveInvalidHeatNumbers()
        {
            // Arrange
            var input = new HeatLotData
            {
                Loads = new System.Collections.Generic.List<Model_ReceivingLoad>
                {
                    new Model_ReceivingLoad { LoadNumber = 1, HeatLotNumber = "" },
                    new Model_ReceivingLoad { LoadNumber = 2, HeatLotNumber = "INVALID!" }
                }
            };

            _mockValidationService.Setup(s => s.ValidateHeatLotNumber(""))
                .Returns(new Service_ReceivingValidation.ValidationResult 
                { 
                    IsValid = false, 
                    Message = "Heat/Lot number is required" 
                });
            _mockValidationService.Setup(s => s.ValidateHeatLotNumber("INVALID!"))
                .Returns(new Service_ReceivingValidation.ValidationResult 
                { 
                    IsValid = false, 
                    Message = "Heat/Lot number contains invalid characters" 
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
