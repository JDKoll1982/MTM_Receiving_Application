using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.ViewModels;

public sealed class ViewModel_Dunnage_QuickAddTypeDialogTests
{
    [Fact]
    public void AddSpec_ShouldShowFieldNameRequiredError_WhenNameIsBlank()
    {
        var viewModel = CreateViewModel();
        viewModel.NewSpecName = "   ";

        viewModel.AddSpecCommand.Execute(null);

        viewModel.HasSpecEditorError.Should().BeTrue();
        viewModel.SpecEditorError.Should().Be("Field Name is required.");
        viewModel.Specs.Should().BeEmpty();
    }

    [Fact]
    public void AddSpec_ShouldRejectNumberDefaultAboveMax()
    {
        var viewModel = CreateViewModel();
        viewModel.NewSpecName = "Weight";
        viewModel.NewSpecType = "Number";
        viewModel.NewSpecMaxValue = 15;
        viewModel.NewSpecDefaultValue = "16";

        viewModel.AddSpecCommand.Execute(null);

        viewModel.SpecEditorError.Should().Contain("cannot be greater than Max Value");
        viewModel.Specs.Should().BeEmpty();
    }

    [Fact]
    public void AddSpec_ShouldRejectNumberDefaultBelowMin()
    {
        var viewModel = CreateViewModel();
        viewModel.NewSpecName = "Weight";
        viewModel.NewSpecType = "Number";
        viewModel.NewSpecMinValue = 1;
        viewModel.NewSpecDefaultValue = "0";

        viewModel.AddSpecCommand.Execute(null);

        viewModel.SpecEditorError.Should().Contain("cannot be less than Min Value");
        viewModel.Specs.Should().BeEmpty();
    }

    [Fact]
    public void AddSpec_ShouldRejectNonNumericNumberDefault()
    {
        var viewModel = CreateViewModel();
        viewModel.NewSpecName = "Weight";
        viewModel.NewSpecType = "Number";
        viewModel.NewSpecDefaultValue = "abc";

        viewModel.AddSpecCommand.Execute(null);

        viewModel.SpecEditorError.Should().Be("Default Value must be a number.");
        viewModel.Specs.Should().BeEmpty();
    }

    [Fact]
    public void AddSpec_ShouldRejectWhenMinIsGreaterThanMax()
    {
        var viewModel = CreateViewModel();
        viewModel.NewSpecName = "Weight";
        viewModel.NewSpecType = "Number";
        viewModel.NewSpecMinValue = 15;
        viewModel.NewSpecMaxValue = 1;
        viewModel.NewSpecDefaultValue = "5";

        viewModel.AddSpecCommand.Execute(null);

        viewModel.SpecEditorError.Should().Be("Min Value cannot be greater than Max Value.");
        viewModel.Specs.Should().BeEmpty();
    }

    [Fact]
    public void AddSpec_ShouldAddNumberSpec_WhenDefaultIsWithinRange()
    {
        var viewModel = CreateViewModel();
        viewModel.NewSpecName = "Weight";
        viewModel.NewSpecType = "Number";
        viewModel.NewSpecMinValue = 1;
        viewModel.NewSpecMaxValue = 15;
        viewModel.NewSpecDefaultValue = "12.5";
        viewModel.NewSpecUnit = "Inches";

        viewModel.AddSpecCommand.Execute(null);

        viewModel.HasSpecEditorError.Should().BeFalse();
        viewModel.Specs.Should().ContainSingle();
        var spec = viewModel.Specs[0];
        spec.Name.Should().Be("Weight");
        spec.DefaultValue.Should().Be("12.5");
        spec.MinValue.Should().Be(1);
        spec.MaxValue.Should().Be(15);
        spec.Unit.Should().Be("Inches");
    }

    [Fact]
    public void AddSpec_ShouldClampBlankNumberDefaultIntoRange()
    {
        var viewModel = CreateViewModel();
        viewModel.NewSpecName = "Weight";
        viewModel.NewSpecType = "Number";
        viewModel.NewSpecMinValue = 1;
        viewModel.NewSpecMaxValue = 15;
        viewModel.NewSpecDefaultValue = string.Empty;

        viewModel.AddSpecCommand.Execute(null);

        viewModel.Specs.Should().ContainSingle();
        viewModel.Specs[0].DefaultValue.Should().Be("1");
    }

    private static ViewModel_Dunnage_QuickAddTypeDialog CreateViewModel()
    {
        return new ViewModel_Dunnage_QuickAddTypeDialog(
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }
}
