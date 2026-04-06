using System.Collections.Generic;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.ViewModels;

public sealed class ViewModel_ShipRecTools_ToolSelectionTests
{
    [Fact]
    public void LoadTools_ShouldReplaceCollectionsAndUpdateVisibilityFlags()
    {
        var navigationServiceMock = new Mock<IService_ShipRecTools_Navigation>();
        navigationServiceMock
            .Setup(service => service.GetToolsByCategory(Enum_ToolCategory.Lookup))
            .Returns(
                new List<Model_ToolDefinition>
                {
                    new() { ToolKey = "lookup-1", Title = "Lookup 1" },
                }
            );
        navigationServiceMock
            .Setup(service => service.GetToolsByCategory(Enum_ToolCategory.Analysis))
            .Returns(new List<Model_ToolDefinition>());
        navigationServiceMock
            .Setup(service => service.GetToolsByCategory(Enum_ToolCategory.Utilities))
            .Returns(
                new List<Model_ToolDefinition>
                {
                    new() { ToolKey = "utility-1", Title = "Utility 1" },
                    new() { ToolKey = "utility-2", Title = "Utility 2" },
                }
            );

        var viewModel = new ViewModel_ShipRecTools_ToolSelection(
            navigationServiceMock.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );

        viewModel.LoadTools();

        viewModel.LookupTools.Should().ContainSingle();
        viewModel.AnalysisTools.Should().BeEmpty();
        viewModel.UtilityTools.Should().HaveCount(2);
        viewModel.HasLookupTools.Should().BeTrue();
        viewModel.HasAnalysisTools.Should().BeFalse();
        viewModel.HasUtilityTools.Should().BeTrue();
    }
}