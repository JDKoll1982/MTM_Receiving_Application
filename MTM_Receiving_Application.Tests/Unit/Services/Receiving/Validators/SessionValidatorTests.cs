using Moq;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.Services.Receiving.Validators;
using System.Collections.Generic;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Services.Receiving.Validators
{
    public class SessionValidatorTests
    {
        private readonly Mock<IService_ReceivingValidation> _mockValidationService;
        private readonly SessionValidator _validator;

        public SessionValidatorTests()
        {
            _mockValidationService = new Mock<IService_ReceivingValidation>();
            _validator = new SessionValidator(_mockValidationService.Object);
        }

        [Fact]
        public async void ValidateAsync_ShouldReturnFailure_WhenLoadsIsNull()
        {
            // Act
            var result = await _validator.ValidateAsync(null!);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Session must contain at least one load", result.ErrorMessage);
        }

        [Fact]
        public async void ValidateAsync_ShouldReturnFailure_WhenLoadsIsEmpty()
        {
            // Arrange
            var loads = new List<Model_ReceivingLoad>();

            // Act
            var result = await _validator.ValidateAsync(loads);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Session must contain at least one load", result.ErrorMessage);
        }

        [Fact]
        public async void ValidateAsync_ShouldReturnSuccess_WhenSessionIsValid()
        {
            // Arrange
            var loads = new List<Model_ReceivingLoad>
            {
                new Model_ReceivingLoad 
                { 
                    LoadNumber = 1, 
                    PartID = "PART-001",
                    WeightQuantity = 100,
                    HeatLotNumber = "HEAT-001",
                    PackageTypeName = "Coils",
                    PackagesPerLoad = 5
                }
            };

            _mockValidationService.Setup(s => s.ValidateSession(loads))
                .Returns(new Service_ReceivingValidation.SessionValidationResult 
                { 
                    IsValid = true 
                });

            // Act
            var result = await _validator.ValidateAsync(loads);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public async void ValidateAsync_ShouldReturnFailure_WhenSessionValidationFails()
        {
            // Arrange
            var loads = new List<Model_ReceivingLoad>
            {
                new Model_ReceivingLoad 
                { 
                    LoadNumber = 1, 
                    PartID = "",
                    WeightQuantity = 0
                }
            };

            _mockValidationService.Setup(s => s.ValidateSession(loads))
                .Returns(new Service_ReceivingValidation.SessionValidationResult 
                { 
                    IsValid = false,
                    Errors = new List<string> 
                    { 
                        "Load 1: Part ID is required",
                        "Load 1: Weight must be greater than zero"
                    }
                });

            // Act
            var result = await _validator.ValidateAsync(loads);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(2, result.Errors.Count);
            Assert.Contains("Part ID is required", result.ErrorMessage);
            Assert.Contains("Weight must be greater than zero", result.ErrorMessage);
        }

        [Fact]
        public async void ValidateAsync_ShouldCallValidateSessionOnService()
        {
            // Arrange
            var loads = new List<Model_ReceivingLoad>
            {
                new Model_ReceivingLoad { LoadNumber = 1 }
            };

            _mockValidationService.Setup(s => s.ValidateSession(loads))
                .Returns(new Service_ReceivingValidation.SessionValidationResult { IsValid = true });

            // Act
            await _validator.ValidateAsync(loads);

            // Assert
            _mockValidationService.Verify(s => s.ValidateSession(loads), Times.Once);
        }
    }
}
