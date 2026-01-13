using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Receiving.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Models
{
    /// <summary>
    /// Unit tests for Model_ReceivingSession.
    /// Tests aggregation logic and transient properties.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Model")]
    public class Model_ReceivingSession_Tests
    {
        [Fact]
        public void Constructor_Defaults_AreCorrect()
        {
            // Act
            var session = new Model_ReceivingSession();

            // Assert
            session.SessionID.Should().NotBeEmpty();
            session.CreatedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
            session.Loads.Should().NotBeNull().And.BeEmpty();
            session.TotalLoadsCount.Should().Be(0);
            session.TotalWeightQuantity.Should().Be(0);
            session.HasLoads.Should().BeFalse();
        }

        [Fact]
        public void TotalWeightQuantity_CalculatesSumCorrectly()
        {
            // Arrange
            var session = new Model_ReceivingSession();
            session.Loads.Add(new Model_ReceivingLoad { WeightQuantity = 100 });
            session.Loads.Add(new Model_ReceivingLoad { WeightQuantity = 250 });

            // Act
            var total = session.TotalWeightQuantity;

            // Assert
            total.Should().Be(350);
        }

        [Fact]
        public void UniqueParts_ReturnsDistinctList()
        {
            // Arrange
            var session = new Model_ReceivingSession();
            session.Loads.Add(new Model_ReceivingLoad { PartID = "PART-A" });
            session.Loads.Add(new Model_ReceivingLoad { PartID = "PART-A" });
            session.Loads.Add(new Model_ReceivingLoad { PartID = "PART-B" });

            // Act
            var parts = session.UniqueParts;

            // Assert
            parts.Should().HaveCount(2);
            parts.Should().Contain("PART-A");
            parts.Should().Contain("PART-B");
        }

        [Fact]
        public void HasLoads_ReturnsTrueWhenLoadsExist()
        {
            // Arrange
            var session = new Model_ReceivingSession();
            session.Loads.Add(new Model_ReceivingLoad());

            // Act
            var hasLoads = session.HasLoads;

            // Assert
            hasLoads.Should().BeTrue();
        }

        [Fact]
        public void TransientProperties_HandleNullLoadsForSafety()
        {
            // Arrange
            var session = new Model_ReceivingSession
            {
                Loads = null! // Simulating deserialization issue or explicit null set
            };

            // Assert
            session.TotalLoadsCount.Should().Be(0);
            session.TotalWeightQuantity.Should().Be(0);
            session.UniqueParts.Should().NotBeNull().And.BeEmpty();
            session.HasLoads.Should().BeFalse();
        }
    }
}
