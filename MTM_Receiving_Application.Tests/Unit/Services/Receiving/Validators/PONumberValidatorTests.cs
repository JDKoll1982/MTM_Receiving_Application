using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.Models.Receiving.StepData;
using MTM_Receiving_Application.Services.Receiving.Validators;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Services.Receiving.Validators
{
    public class PONumberValidatorTests
    {
        private readonly PONumberValidator _validator;

        public PONumberValidatorTests()
        {
            _validator = new PONumberValidator();
        }

        [Fact]
        public async void ValidateAsync_ShouldReturnFailure_WhenInputIsNull()
        {
            // Act
            var result = await _validator.ValidateAsync(null!);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("PO Entry data is required", result.ErrorMessage);
        }

        [Fact]
        public async void ValidateAsync_ShouldReturnFailure_WhenPONumberMissingForPOItem()
        {
            // Arrange
            var input = new POEntryData
            {
                PONumber = null,
                IsNonPOItem = false,
                SelectedPart = null
            };

            // Act
            var result = await _validator.ValidateAsync(input);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("PO Number is required", result.ErrorMessage);
        }

        [Fact]
        public async void ValidateAsync_ShouldReturnFailure_WhenPartNotSelected()
        {
            // Arrange
            var input = new POEntryData
            {
                PONumber = "12345",
                IsNonPOItem = false,
                SelectedPart = null
            };

            // Act
            var result = await _validator.ValidateAsync(input);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Part selection is required", result.ErrorMessage);
        }

        [Fact]
        public async void ValidateAsync_ShouldReturnSuccess_WhenPONumberAndPartProvided()
        {
            // Arrange
            var input = new POEntryData
            {
                PONumber = "12345",
                IsNonPOItem = false,
                SelectedPart = new Model_InforVisualPart { PartID = "PART-001" }
            };

            // Act
            var result = await _validator.ValidateAsync(input);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public async void ValidateAsync_ShouldReturnSuccess_WhenNonPOItemAndPartProvided()
        {
            // Arrange
            var input = new POEntryData
            {
                PONumber = null,
                IsNonPOItem = true,
                SelectedPart = new Model_InforVisualPart { PartID = "PART-001" }
            };

            // Act
            var result = await _validator.ValidateAsync(input);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public async void ValidateAsync_ShouldReturnFailure_WhenNonPOItemButNoPartSelected()
        {
            // Arrange
            var input = new POEntryData
            {
                PONumber = null,
                IsNonPOItem = true,
                SelectedPart = null
            };

            // Act
            var result = await _validator.ValidateAsync(input);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Part selection is required", result.ErrorMessage);
        }
    }
}
