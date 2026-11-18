using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.ViewModels;

public sealed class ViewModel_Dunnage_QuantityEntryTests
{
    [Fact]
    public void LoadContextData_ShouldShowPartSummaryAndTotalOfQuantityPerLoad()
    {
        var session = CreateSession();
        var viewModel = CreateViewModel(session);

        viewModel.LoadContextData();

        viewModel.CurrentTypeName.Should().Be("Bins");
        viewModel.CurrentPartId.Should().Be("PART-900");
        viewModel.CurrentHomeLocationDisplay.Should().Be("A-12");
        viewModel.CurrentQuantityType.Should().Be("Totes");
        viewModel.CurrentLoadCount.Should().Be(3);
        viewModel.CurrentTotal.Should().Be("20");
    }

    [Fact]
    public void LoadQuantityChange_ShouldUpdateRunningTotal()
    {
        var session = CreateSession();
        var viewModel = CreateViewModel(session);

        viewModel.LoadContextData();

        viewModel.Loads[2].Quantity = 10;

        viewModel.CurrentTotal.Should().Be("26");
    }

    [Fact]
    public void LoadContextData_ShouldFallBackWhenPartHasNoHomeLocationOrQuantityType()
    {
        var session = new Model_DunnageSession
        {
            SelectedTypeName = "Bins",
            SelectedPart = new Model_DunnagePart { PartId = "PART-901", QuantityType = string.Empty },
            NumberOfLoads = 1,
        };
        session.LoadQuantities.Add(5);

        var viewModel = CreateViewModel(session);

        viewModel.LoadContextData();

        viewModel.CurrentHomeLocationDisplay.Should().Be("Not set");
        viewModel.CurrentQuantityType.Should().Be("Quantity");
        viewModel.HasCurrentVisual.Should().BeFalse();
        viewModel.CurrentTotal.Should().Be("5");
    }

    private static Model_DunnageSession CreateSession()
    {
        var session = new Model_DunnageSession
        {
            SelectedTypeName = "Bins",
            SelectedPart = new Model_DunnagePart
            {
                PartId = "PART-900",
                HomeLocation = "A-12",
                QuantityType = "Totes",
            },
            NumberOfLoads = 3,
        };

        session.LoadQuantities.Add(8);
        session.LoadQuantities.Add(8);
        session.LoadQuantities.Add(4);

        return session;
    }

    private static ViewModel_Dunnage_QuantityEntry CreateViewModel(Model_DunnageSession session)
    {
        var workflow = new Mock<IService_DunnageWorkflow>();
        workflow.SetupGet(service => service.CurrentSession).Returns(session);
        workflow.SetupGet(service => service.NumberOfLoads).Returns(session.NumberOfLoads);

        return new ViewModel_Dunnage_QuantityEntry(
            workflow.Object,
            new Mock<IService_Dispatcher>().Object,
            new Mock<IService_Help>().Object,
            new Mock<IService_ViewModelRegistry>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }
}
