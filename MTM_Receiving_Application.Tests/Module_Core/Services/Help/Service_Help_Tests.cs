using System;
using System.Linq;
using Xunit;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Services.Help;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Tests.Module_Core.Services.Help
{
    /// <summary>
    /// Unit tests for Service_Help content retrieval
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Service")]
    public class Service_Help_Tests
    {
        private readonly Mock<IService_Window> _mockWindow;
        private readonly Mock<IService_LoggingUtility> _mockLogger;
        private readonly Mock<IService_Dispatcher> _mockDispatcher;
        private readonly Service_Help _sut;

        public Service_Help_Tests()
        {
            _mockWindow = new Mock<IService_Window>();
            _mockLogger = new Mock<IService_LoggingUtility>();
            _mockDispatcher = new Mock<IService_Dispatcher>();

        _sut = new Service_Help(_mockWindow.Object, _mockLogger.Object, _mockDispatcher.Object);
        }

        [Fact]
        public void GetHelpContent_ExistingKey_ReturnsContent()
        {
            // Act
            var content = _sut.GetHelpContent("Dunnage.ModeSelection");

            // Assert
            content.Should().NotBeNull();
            content?.Title.Should().Be("Select Entry Mode");
            content?.Key.Should().Be("Dunnage.ModeSelection");
        }

        [Fact]
        public void GetHelpContent_NonExistentKey_ReturnsNull()
        {
            // Act
            var content = _sut.GetHelpContent("NonExistentKey");

            // Assert
            content.Should().BeNull();
        }

        [Fact]
        public void GetHelpByCategory_ReturnsFilteredList()
        {
            // Act
            var list = _sut.GetHelpByCategory("Dunnage Workflow");

            // Assert
            list.Should().NotBeEmpty();
            list.All(x => x.Category == "Dunnage Workflow").Should().BeTrue();
        }

        [Fact]
        public void SearchHelp_ReturnsMatchingItems()
        {
            // Act
            var results = _sut.SearchHelp("dunnage");

            // Assert
            results.Should().NotBeEmpty();
            // Should contain items with "dunnage" in title or content
            results.Should().Contain(x => x.Title.ToLower().Contains("dunnage") || x.Content.ToLower().Contains("dunnage"));
        }

        [Fact]
        public void SearchHelp_EmptyTerm_ReturnsEmptyList()
        {
            // Act
            var results = _sut.SearchHelp("");

            // Assert
            results.Should().BeEmpty();
        }

         [Fact]
        public void TipRetreival_ReturnsContent()
        {
            // Act
            var tip = _sut.GetTip("Dunnage.QuantityEntry");

            // Assert
            tip.Should().NotBeNullOrEmpty();
        }
    }
}
