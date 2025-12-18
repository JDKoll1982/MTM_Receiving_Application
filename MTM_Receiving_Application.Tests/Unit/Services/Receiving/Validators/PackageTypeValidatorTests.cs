using Moq;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.Models.Receiving.StepData;
using MTM_Receiving_Application.Services.Receiving.Validators;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Services.Receiving.Validators
{
    public class PackageTypeValidatorTests
    {
        private readonly Mock<IService_ReceivingValidation> _mockValidationService;
        private readonly PackageTypeValidator _validator;

        public PackageTypeValidatorTests()
        {
            _mockValidationService = new Mock<IService_ReceivingValidation>();
            _validator = new PackageTypeValidator(_mockValidationService.Object);
        }

        [Fact]
        public async void ValidateAsync_ShouldReturnFailure_WhenInputIsNull()
        {
            // Act
            var result = await _validator.ValidateAsync(null!);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Package Type data is required", result.ErrorMessage);
        }

        [Fact]
        public async void ValidateAsync_ShouldReturnSuccess_WhenAllLoadsHaveValidPackageData()
        {
            // Arrange
            var input = new PackageTypeData
            {
                Loads = new System.Collections.Generic.List<Model_ReceivingLoad>
                {
                    new Model_ReceivingLoad 
                    { 
                        LoadNumber = 1, 
                        PackageTypeName = "Coils", 
                        PackagesPerLoad = 5 
                    },
                    new Model_ReceivingLoad 
                    { 
                        LoadNumber = 2, 
                        PackageTypeName = "Sheets", 
                        PackagesPerLoad = 10 
                    }
                }
            };

            _mockValidationService.Setup(s => s.ValidatePackageCount(It.IsAny<int>()))
                .Returns(new Service_ReceivingValidation.ValidationResult { IsValid = true });

            // Act
            var result = await _validator.ValidateAsync(input);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public async void ValidateAsync_ShouldReturnFailure_WhenPackageCountIsInvalid()
        {
            // Arrange
            var input = new PackageTypeData
            {
                Loads = new System.Collections.Generic.List<Model_ReceivingLoad>
                {
                    new Model_ReceivingLoad 
                    { 
                        LoadNumber = 1, 
                        PackageTypeName = "Coils", 
                        PackagesPerLoad = 0 
                    }
                }
            };

            _mockValidationService.Setup(s => s.ValidatePackageCount(0))
                .Returns(new Service_ReceivingValidation.ValidationResult 
                { 
                    IsValid = false, 
                    Message = "Package count must be at least 1" 
                });

            // Act
            var result = await _validator.ValidateAsync(input);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Load 1", result.ErrorMessage);
            Assert.Contains("Package count must be at least 1", result.ErrorMessage);
        }

        [Fact]
        public async void ValidateAsync_ShouldReturnFailure_WhenPackageTypeNameIsMissing()
        {
            // Arrange
            var input = new PackageTypeData
            {
                Loads = new System.Collections.Generic.List<Model_ReceivingLoad>
                {
                    new Model_ReceivingLoad 
                    { 
                        LoadNumber = 1, 
                        PackageTypeName = "", 
                        PackagesPerLoad = 5 
                    }
                }
            };

            _mockValidationService.Setup(s => s.ValidatePackageCount(5))
                .Returns(new Service_ReceivingValidation.ValidationResult { IsValid = true });

            // Act
            var result = await _validator.ValidateAsync(input);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Load 1", result.ErrorMessage);
            Assert.Contains("Package Type is required", result.ErrorMessage);
        }

        [Fact]
        public async void ValidateAsync_ShouldReturnMultipleErrors_WhenMultipleLoadsHaveIssues()
        {
            // Arrange
            var input = new PackageTypeData
            {
                Loads = new System.Collections.Generic.List<Model_ReceivingLoad>
                {
                    new Model_ReceivingLoad 
                    { 
                        LoadNumber = 1, 
                        PackageTypeName = "Coils", 
                        PackagesPerLoad = -1 
                    },
                    new Model_ReceivingLoad 
                    { 
                        LoadNumber = 2, 
                        PackageTypeName = "", 
                        PackagesPerLoad = 5 
                    }
                }
            };

            _mockValidationService.Setup(s => s.ValidatePackageCount(-1))
                .Returns(new Service_ReceivingValidation.ValidationResult 
                { 
                    IsValid = false, 
                    Message = "Package count cannot be negative" 
                });
            _mockValidationService.Setup(s => s.ValidatePackageCount(5))
                .Returns(new Service_ReceivingValidation.ValidationResult { IsValid = true });

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
