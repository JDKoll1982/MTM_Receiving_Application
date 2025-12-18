using MTM_Receiving_Application.Models.Receiving.StepData;
using MTM_Receiving_Application.Services.Receiving.Validators;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Services.Receiving.Validators
{
    public class LoadCountValidatorTests
    {
        private readonly LoadCountValidator _validator;

        public LoadCountValidatorTests()
        {
            _validator = new LoadCountValidator();
        }

        [Fact]
        public async void ValidateAsync_ShouldReturnFailure_WhenInputIsNull()
        {
            // Act
            var result = await _validator.ValidateAsync(null!);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Load Entry data is required", result.ErrorMessage);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-10)]
        public async void ValidateAsync_ShouldReturnFailure_WhenNumberOfLoadsIsLessThanOne(int numberOfLoads)
        {
            // Arrange
            var input = new LoadEntryData
            {
                NumberOfLoads = numberOfLoads
            };

            // Act
            var result = await _validator.ValidateAsync(input);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Number of loads must be at least 1", result.ErrorMessage);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(100)]
        public async void ValidateAsync_ShouldReturnSuccess_WhenNumberOfLoadsIsValid(int numberOfLoads)
        {
            // Arrange
            var input = new LoadEntryData
            {
                NumberOfLoads = numberOfLoads
            };

            // Act
            var result = await _validator.ValidateAsync(input);

            // Assert
            Assert.True(result.IsValid);
        }
    }
}
