using System.Data;
using MySql.Data.MySqlClient;
using Xunit;
using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Helpers.Database;

namespace MTM_Receiving_Application.Tests.Module_Core.Helpers.Database
{
    /// <summary>
    /// Unit tests for Helper_Database_StoredProcedure logic
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Helper")]
    public class Helper_Database_StoredProcedure_Tests
    {
        [Fact]
        public void ValidateParameters_NullParameters_ReturnsTrue()
        {
            // Act
            var result = Helper_Database_StoredProcedure.ValidateParameters(null);

            // Assert
            result.Should().BeTrue("null parameters list is considered valid (no parameters to validate)");
        }

        [Fact]
        public void ValidateParameters_ValidInputParameters_ReturnsTrue()
        {
            // Arrange
            var parameters = new[]
            {
                new MySqlParameter("@p1", "value"),
                new MySqlParameter("@p2", 123)
            };

            // Act
            var result = Helper_Database_StoredProcedure.ValidateParameters(parameters);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ValidateParameters_NullInputParameterContent_ReturnsFalse()
        {
            // Arrange
            // Create a parameter with explicit null value (not DBNull)
            var parameter = new MySqlParameter("@p1", MySqlDbType.VarChar);
            parameter.Value = null;
            parameter.Direction = ParameterDirection.Input;

            var parameters = new[] { parameter };

            // Act
            var result = Helper_Database_StoredProcedure.ValidateParameters(parameters);

            // Assert
            result.Should().BeFalse("input parameters should not be null (use DBNull.Value instead)");
        }

        [Fact]
        public void ValidateParameters_DBNullInputParameter_ReturnsTrue()
        {
            // Arrange
            var parameters = new[]
            {
                new MySqlParameter("@p1", System.DBNull.Value)
            };

            // Act
            var result = Helper_Database_StoredProcedure.ValidateParameters(parameters);

            // Assert
            result.Should().BeTrue("DBNull.Value is a valid value for parameters");
        }
    }
}
