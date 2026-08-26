using System.ComponentModel;
using System.Threading;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Scanner.Models;
using MTM_Receiving_Application.Module_Scanner.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_Scanner.ViewModels;

public sealed class ViewModel_Scanner_WorkbenchTests
{
    // ── ScannerUpdate.md Task 6: baseline input states ───────────────────────────

    [Fact]
    public void BaselineInputStates_ShouldMatchScannerUpdateTask6()
    {
        var viewModel = CreateWorkbenchViewModel();

        viewModel.IsPartEditable.Should().BeTrue();
        viewModel.IsFromLocationReadOnly.Should().BeTrue();
        viewModel.IsToLocationReadOnly.Should().BeTrue();
        viewModel.IsToLocationEnabled.Should().BeFalse();
        viewModel.IsQuantityReadOnly.Should().BeTrue();
        viewModel.IsQuantityEnabled.Should().BeFalse();
        viewModel.IsAddEnabled.Should().BeFalse();
    }

    // ── ScannerUpdate.md Step 6b-a1-c-a/b: To Location format sanitization ──────

    [Fact]
    public async Task ValidateToLocationFormatAsync_ShouldSanitizeAndReturnTrue()
    {
        var viewModel = CreateWorkbenchViewModel();
        viewModel.NewToLocation = "VA-A001";

        var isValid = await viewModel.ValidateToLocationFormatAsync();

        isValid.Should().BeTrue();
        viewModel.NewToLocation.Should().Be("VA-A0-01");
        viewModel.IsHeaderErrorVisible.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateToLocationFormatAsync_ShouldShowHeaderError_WhenInvalid()
    {
        var viewModel = CreateWorkbenchViewModel();
        viewModel.NewToLocation = "A";

        var isValid = await viewModel.ValidateToLocationFormatAsync();

        isValid.Should().BeFalse();
        viewModel.IsHeaderErrorVisible.Should().BeTrue();
        viewModel.HeaderErrorText.Should().NotBeNullOrWhiteSpace();
    }

    // ── ScannerUpdate.md Step 6b-a2: no-stock header error + part refocus ───────

    [Fact]
    public async Task PartValidationCompletedAsync_WithNoStock_ShouldShowHeaderErrorAndRefocusPart()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        var validation = new Mock<IService_ScannerValidation>();
        validation
            .Setup(service => service.PartExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(true));
        validation
            .Setup(service =>
                service.GetLocationsWithStockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<IReadOnlyList<Model_InforVisualMaterialLocationRow>>([])
            );

        var viewModel = CreateWorkbenchViewModel(workflow.Object, validation.Object);
        var partFocusRaised = false;
        viewModel.PartIdFocusRequested += () => partFocusRaised = true;

        await viewModel.PartValidationCompletedAsync("MMCCS00740");

        viewModel.IsHeaderErrorVisible.Should().BeTrue();
        viewModel.HeaderErrorText.Should().Contain("does not have any quantity");
        partFocusRaised.Should().BeTrue();
    }

    [Fact]
    public async Task PartValidationCompletedAsync_WithStock_ShouldApplySelectionAndEnableToLocation()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        var validation = new Mock<IService_ScannerValidation>();
        validation
            .Setup(service => service.PartExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(true));
        validation
            .Setup(service =>
                service.GetLocationsWithStockAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<IReadOnlyList<Model_InforVisualMaterialLocationRow>>(
                [
                    new Model_InforVisualMaterialLocationRow
                    {
                        LocationId = "V-A1-01",
                        WarehouseCode = "002",
                        Quantity = 25m,
                    },
                ])
            );

        var viewModel = CreateWorkbenchViewModel(workflow.Object, validation.Object);
        viewModel.FromLocationInventoryPickerRequested += (_, _, _) =>
            Task.FromResult<Model_ScannerStockPick?>(
                new Model_ScannerStockPick { Location = "V-A1-01", Quantity = "25" }
            );

        await viewModel.PartValidationCompletedAsync("MMCCS00740");

        viewModel.NewFromLocation.Should().Be("V-A1-01");
        viewModel.NewQuantity.Should().Be("25");
        viewModel.IsToLocationEnabled.Should().BeTrue();
        viewModel.IsHeaderErrorVisible.Should().BeFalse();
    }

    [Fact]
    public async Task PartValidationCompletedAsync_WhenPartMissing_ShouldShowHeaderError()
    {
        var validation = new Mock<IService_ScannerValidation>();
        validation
            .Setup(service => service.PartExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(false));

        var viewModel = CreateWorkbenchViewModel(validation: validation.Object);

        await viewModel.PartValidationCompletedAsync("NOPE");

        viewModel.IsHeaderErrorVisible.Should().BeTrue();
    }

    // ── Header error (non-blocking, clearable) ──────────────────────────────────

    [Fact]
    public void ClearHeaderError_ShouldHideImmediately()
    {
        var viewModel = CreateWorkbenchViewModel();
        viewModel.ShowHeaderError("Some problem");

        viewModel.IsHeaderErrorVisible.Should().BeTrue();

        viewModel.ClearHeaderError();

        viewModel.IsHeaderErrorVisible.Should().BeFalse();
        viewModel.HeaderErrorText.Should().BeEmpty();
    }

    // ── ScannerUpdate.md Task 1/2b: input lockout during automation ─────────────

    [Fact]
    public void InputLockout_ShouldDisableInputs_WhenAutomationRuns()
    {
        var execution = new Mock<IService_ScannerExecution>();
        var viewModel = CreateWorkbenchViewModel(execution: execution.Object);

        execution.SetupGet(service => service.IsAutomationRunning).Returns(true);
        execution.Raise(
            service => service.PropertyChanged += null,
            new PropertyChangedEventArgs(nameof(IService_ScannerExecution.IsAutomationRunning))
        );

        viewModel.IsPartEditable.Should().BeFalse();
        viewModel.IsAddEnabled.Should().BeFalse();
    }

    // ── Clear List ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task ClearListCommand_ShouldEmptyTheCurrentList()
    {
        var workflow = new Mock<IService_ScannerWorkflow>();
        var session = new Model_ScannerBatchSession { SessionId = Guid.NewGuid() };
        session.Items.Add(
            new Model_ScannerBatchItem
            {
                ItemId = Guid.NewGuid(),
                SessionId = session.SessionId,
                SequenceNumber = 1,
                PayloadPartId = "MMCCS00740",
            }
        );
        session.RecalculateItemCounters();

        workflow
            .Setup(service =>
                service.EnsureCurrentSessionAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Model_Dao_Result_Factory.Success(session));
        workflow
            .Setup(service =>
                service.GetProfilesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_ScannerProfile>())
            );

        var viewModel = CreateWorkbenchViewModel(workflow.Object);
        await viewModel.EnsureCurrentSessionAsync();

        viewModel.SessionItems.Should().HaveCount(1);

        await viewModel.ClearListCommand.ExecuteAsync(null);

        viewModel.SessionItems.Should().BeEmpty();
        viewModel.HasActiveSession.Should().BeTrue();
    }

    // ── Helpers ─────────────────────────────────────────────────────────────────

    private static ViewModel_Scanner_Workbench CreateWorkbenchViewModel(
        IService_ScannerWorkflow? workflow = null,
        IService_ScannerValidation? validation = null,
        IService_ScannerExecution? execution = null
    )
    {
        return new ViewModel_Scanner_Workbench(
            new Mock<IService_ScannerNavigation>().Object,
            workflow ?? new Mock<IService_ScannerWorkflow>().Object,
            validation ?? new Mock<IService_ScannerValidation>().Object,
            execution ?? new Mock<IService_ScannerExecution>().Object,
            new Mock<IService_ScannerHotkey>().Object,
            new Mock<IService_Window>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }
}
