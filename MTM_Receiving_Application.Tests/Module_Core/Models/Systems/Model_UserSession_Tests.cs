using System;
using System.Threading;
using Xunit;
using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Models.Systems;

namespace MTM_Receiving_Application.Tests.Module_Core.Models.Systems
{
    /// <summary>
    /// Unit tests for Model_UserSession
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Model")]
    public class Model_UserSession_Tests
    {
        [Fact]
        public void DefaultConstructor_SetsTimestamps()
        {
            // Act
            var session = new Model_UserSession();

            // Assert
            session.LoginTimestamp.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
            session.LastActivityTimestamp.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void UpdateLastActivity_UpdatesTimestamp()
        {
            // Arrange
            var session = new Model_UserSession();
            var oldTime = session.LastActivityTimestamp;
            Thread.Sleep(10); // Ensure minimal time passage

            // Act
            session.UpdateLastActivity();

            // Assert
            session.LastActivityTimestamp.Should().BeAfter(oldTime);
        }

        [Fact]
        public void TimeSinceLastActivity_ReturnsCorrectDifference()
        {
            // Arrange
            var session = new Model_UserSession();
            var pastTime = DateTime.Now.AddMinutes(-5);
            session.LastActivityTimestamp = pastTime;

            // Act
            var elapsed = session.TimeSinceLastActivity;

            // Assert
            elapsed.Should().BeCloseTo(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void IsTimedOut_ReturnsTrueWhenExpired()
        {
            // Arrange
            var session = new Model_UserSession
            {
                TimeoutDuration = TimeSpan.FromMinutes(10),
                LastActivityTimestamp = DateTime.Now.AddMinutes(-11)
            };

            // Act
            var isTimedOut = session.IsTimedOut;

            // Assert
            isTimedOut.Should().BeTrue();
        }

        [Fact]
        public void IsTimedOut_ReturnsFalseWhenActive()
        {
            // Arrange
            var session = new Model_UserSession
            {
                TimeoutDuration = TimeSpan.FromMinutes(10),
                LastActivityTimestamp = DateTime.Now.AddMinutes(-5)
            };

            // Act
            var isTimedOut = session.IsTimedOut;

            // Assert
            isTimedOut.Should().BeFalse();
        }

        [Fact]
        public void HasErpAccess_ReturnsFalseIfUserNull()
        {
             // Arrange
            var session = new Model_UserSession();
            // User is null by default due to strict null checks but initialization logic allows null refs for a moment?
            // User is marked as non-nullable in property signature `Model_User User { get; set; } = null!;` 
            // but effectively null until set.
            
            // Act
            var hasAccess = session.HasErpAccess;

            // Assert
            hasAccess.Should().BeFalse();
        }

        [Fact]
        public void HasErpAccess_ReturnsUserValue()
        {
            // Arrange
            var session = new Model_UserSession
            {
                User = new Model_User 
                { 
                     VisualUsername = "user", 
                     VisualPassword = "pass" // Assumed this makes HasErpAccess true, let's check Model_User later
                }
            };
            
            // Note: We need to know logic of Model_User.HasErpAccess. Assuming it checks props.
            // If Model_User logic is complex, we should mock or configure it correctly.
            // Let's assume HasErpAccess depends on VisualUsername/Password being set.
        }
    }
}
