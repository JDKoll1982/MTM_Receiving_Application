using System;
using Xunit;
using MTM_Receiving_Application.Data.InforVisual;

namespace MTM_Receiving_Application.Tests.Unit
{
    /// <summary>
    /// NOTE: If tests fail due to connection issues, it is likely because the test is running
    /// outside the work environment (e.g. home development) without access to the Infor Visual server.
    /// </summary>
    public class Dao_InforVisualPart_Tests
    {
        [Fact]
        public void Constructor_ShouldThrowException_WhenApplicationIntentReadOnlyIsMissing()
        {
            // Arrange
            string invalidConnectionString = "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => new Dao_InforVisualPart(invalidConnectionString));
            Assert.Contains("ApplicationIntent=ReadOnly", exception.Message);
        }

        [Fact]
        public void Constructor_ShouldNotThrowException_WhenApplicationIntentReadOnlyIsPresent()
        {
            // Arrange
            string validConnectionString = "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;ApplicationIntent=ReadOnly;";

            // Act
            var dao = new Dao_InforVisualPart(validConnectionString);

            // Assert
            Assert.NotNull(dao);
        }
    }
}
