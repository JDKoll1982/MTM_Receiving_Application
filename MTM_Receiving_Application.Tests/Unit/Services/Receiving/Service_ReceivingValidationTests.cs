using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using MTM_Receiving_Application.Services.Receiving;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Tests.Unit.Services.Receiving
{
    public class Service_ReceivingValidationTests
    {
        private readonly Mock<IService_InforVisual> _mockInforVisual;
        private readonly Service_ReceivingValidation _service;

        public Service_ReceivingValidationTests()
        {
            _mockInforVisual = new Mock<IService_InforVisual>();
            _service = new Service_ReceivingValidation(_mockInforVisual.Object);
        }

        [Theory]
        [InlineData("123456", true)]
        [InlineData("1", true)]
        [InlineData("1234567", false)] // Too long
        [InlineData("", false)] // Empty
        [InlineData("ABC", false)] // Non-numeric
        public void ValidatePONumber_ShouldReturnExpectedResult(string poNumber, bool expectedSuccess)
        {
            // Act
            var result = _service.ValidatePONumber(poNumber);

            // Assert
            Assert.Equal(expectedSuccess, result.IsValid);
        }

        [Fact]
        public void ValidatePONumber_ShouldReturnFalse_WhenNull()
        {
            // Act
            var result = _service.ValidatePONumber(null!);

            // Assert
            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("PART-123", true)]
        [InlineData("", false)] // Empty
        public void ValidatePartID_ShouldReturnExpectedResult(string partID, bool expectedSuccess)
        {
            // Act
            var result = _service.ValidatePartID(partID);

            // Assert
            Assert.Equal(expectedSuccess, result.IsValid);
        }

        [Fact]
        public void ValidatePartID_ShouldReturnFalse_WhenNull()
        {
            // Act
            var result = _service.ValidatePartID(null!);

            // Assert
            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(99, true)]
        [InlineData(0, false)] // Too low
        [InlineData(100, false)] // Too high
        public void ValidateNumberOfLoads_ShouldReturnExpectedResult(int numLoads, bool expectedSuccess)
        {
            // Act
            var result = _service.ValidateNumberOfLoads(numLoads);

            // Assert
            Assert.Equal(expectedSuccess, result.IsValid);
        }

        [Theory]
        [InlineData(10.5, true)]
        [InlineData(0, false)] // Zero
        [InlineData(-1, false)] // Negative
        public void ValidateWeightQuantity_ShouldReturnExpectedResult(decimal weight, bool expectedSuccess)
        {
            // Act
            var result = _service.ValidateWeightQuantity(weight);

            // Assert
            Assert.Equal(expectedSuccess, result.IsValid);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(0, false)] // Zero
        [InlineData(-1, false)] // Negative
        public void ValidatePackageCount_ShouldReturnExpectedResult(int count, bool expectedSuccess)
        {
            // Act
            var result = _service.ValidatePackageCount(count);

            // Assert
            Assert.Equal(expectedSuccess, result.IsValid);
        }

        [Theory]
        [InlineData("HEAT-123", true)]
        [InlineData("", false)] // Empty
        public void ValidateHeatLotNumber_ShouldReturnExpectedResult(string heat, bool expectedSuccess)
        {
            // Act
            var result = _service.ValidateHeatLotNumber(heat);

            // Assert
            Assert.Equal(expectedSuccess, result.IsValid);
        }

        [Fact]
        public void ValidateHeatLotNumber_ShouldReturnFalse_WhenNull()
        {
            // Act
            var result = _service.ValidateHeatLotNumber(null!);

            // Assert
            Assert.False(result.IsValid);
        }

        [Fact]
        public async Task ValidateAgainstPOQuantityAsync_ShouldReturnWarning_WhenQuantityExceedsOrdered()
        {
            // Arrange
            decimal totalQty = 110;
            decimal orderedQty = 100;
            string partID = "PART-123";

            // Act
            var result = await _service.ValidateAgainstPOQuantityAsync(totalQty, orderedQty, partID);

            // Assert
            Assert.True(result.IsValid); // Still valid, just a warning
            Assert.Equal(ValidationSeverity.Warning, result.Severity);
            Assert.Contains("exceeds PO ordered quantity", result.Message);
        }

        [Fact]
        public async Task ValidateAgainstPOQuantityAsync_ShouldReturnSuccess_WhenQuantityWithinOrdered()
        {
            // Arrange
            decimal totalQty = 90;
            decimal orderedQty = 100;
            string partID = "PART-123";

            // Act
            var result = await _service.ValidateAgainstPOQuantityAsync(totalQty, orderedQty, partID);

            // Assert
            Assert.True(result.IsValid);
            Assert.NotEqual(ValidationSeverity.Warning, result.Severity);
        }
    }
}
