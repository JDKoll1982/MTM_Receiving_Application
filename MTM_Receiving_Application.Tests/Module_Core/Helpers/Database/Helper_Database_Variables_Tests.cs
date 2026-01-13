using Xunit;
using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Helpers.Database;

namespace MTM_Receiving_Application.Tests.Module_Core.Helpers.Database
{
    /// <summary>
    /// Unit tests for Helper_Database_Variables
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Helper")]
    public class Helper_Database_Variables_Tests
    {
        [Fact]
        public void GetConnectionString_Production_ReturnsProductionString()
        {
            // Act
            var connectionString = Helper_Database_Variables.GetConnectionString(true);

            // Assert
            connectionString.Should().Contain("Database=mtm_receiving_application");
            connectionString.Should().NotContain("_test");
        }

        [Fact]
        public void GetConnectionString_Test_ReturnsTestString()
        {
            // Act
            var connectionString = Helper_Database_Variables.GetConnectionString(false);

            // Assert
            connectionString.Should().Contain("Database=mtm_receiving_application_test");
        }

        [Fact]
        public void GetInforVisualConnectionString_ReturnsReadOnlyIntent()
        {
            // Act
            var connectionString = Helper_Database_Variables.GetInforVisualConnectionString();

            // Assert
            connectionString.Should().Contain("ApplicationIntent=ReadOnly");
        }
    }
}
