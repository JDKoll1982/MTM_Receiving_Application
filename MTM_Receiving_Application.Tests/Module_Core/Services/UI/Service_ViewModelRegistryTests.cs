using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Core.Services.UI;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Core.Services.UI;

[Trait("Category", "Unit")]
[Trait("Layer", "Service")]
[Trait("Module", "Module_Core")]
public class Service_ViewModelRegistryTests
{
    private readonly Service_ViewModelRegistry _sut;

    public Service_ViewModelRegistryTests()
    {
        _sut = new Service_ViewModelRegistry();
    }

    [Fact]
    public void Register_ShouldReturnViewModel_WhenGetViewModelsIsCalled()
    {
        // Arrange
        var viewModel = new TestViewModel();

        // Act
        _sut.Register(viewModel);
        var result = _sut.GetViewModels<TestViewModel>();

        // Assert
        result.Count().Should().Be(1);
    }

    [Fact]
    public void GetViewModels_ShouldReturnEmpty_WhenNoMatchingTypesExist()
    {
        // Arrange
        _sut.Register(new object());

        // Act
        var result = _sut.GetViewModels<TestViewModel>();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ClearAllInputs_ShouldResetViewModel_WhenResettableRegistered()
    {
        // Arrange
        var viewModel = new ResettableViewModel();
        _sut.Register(viewModel);

        // Act
        _sut.ClearAllInputs();

        // Assert
        viewModel.ResetCalled.Should().BeTrue();
    }

    private class TestViewModel
    {
    }

    private sealed class ResettableViewModel : IResettableViewModel
    {
        public bool ResetCalled { get; private set; }

        public void ResetToDefaults()
        {
            ResetCalled = true;
        }
    }
}
