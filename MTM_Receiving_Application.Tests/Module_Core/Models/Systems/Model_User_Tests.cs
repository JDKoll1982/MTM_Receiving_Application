using System;
using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Core.Models.Systems
{
    /// <summary>
    /// Unit tests for Model_User
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Model")]
    public class Model_User_Tests
    {
        [Fact]
        public void Constructor_Default_SetsCreatedAndModifiedToNow()
        {
            // Arrange
            var before = DateTime.Now;

            // Act
            var user = new Model_User();
            var after = DateTime.Now;

            // Assert
            user.CreatedDate.Should().BeOnOrAfter(before);
            user.CreatedDate.Should().BeOnOrBefore(after);
            user.ModifiedDate.Should().BeOnOrAfter(before);
            user.ModifiedDate.Should().BeOnOrBefore(after);
        }

        [Fact]
        public void HasErpAccess_ReturnsFalseWhenVisualUsernameMissing()
        {
            var user = new Model_User { VisualUsername = null };

            user.HasErpAccess.Should().BeFalse();
        }

        [Fact]
        public void HasErpAccess_ReturnsFalseWhenVisualUsernameWhitespace()
        {
            var user = new Model_User { VisualUsername = "   " };

            user.HasErpAccess.Should().BeFalse();
        }

        [Fact]
        public void HasErpAccess_ReturnsTrueWhenVisualUsernameProvided()
        {
            var user = new Model_User { VisualUsername = "user" };

            user.HasErpAccess.Should().BeTrue();
        }

        [Fact]
        public void DisplayName_FormatsEmployeeNumberAndName()
        {
            var user = new Model_User
            {
                FullName = "Jane Doe",
                EmployeeNumber = 123
            };

            user.DisplayName.Should().Be("Jane Doe (Emp #123)");
        }
    }
}
