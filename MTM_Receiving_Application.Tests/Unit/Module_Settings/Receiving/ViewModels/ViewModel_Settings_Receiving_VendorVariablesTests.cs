using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_Settings.Receiving.ViewModels;

public sealed class ViewModel_Settings_Receiving_VendorVariablesTests
{
    [Fact]
    public async Task SearchVendorsAsync_ShouldReturnMatches_WhenInforVisualReturnsResults()
    {
        var viewModel = CreateViewModel();

        var results = await viewModel.SearchVendorsAsync("sk");

        results.Should().ContainSingle();
        results[0].Label.Should().Be("Skana");
    }

    [Fact]
    public async Task LoadMappingForVendorAsync_ShouldSetEditingState_WhenMappingExists()
    {
        var viewModel = CreateViewModel();

        await viewModel.LoadMappingForVendorAsync("Skana");

        viewModel.VendorName.Should().Be("Skana");
        viewModel.VariableName.Should().Be("DM #");
        viewModel.IsEditingExisting.Should().BeTrue();
    }

    [Fact]
    public async Task LoadMappingForVendorAsync_ShouldClearVariable_WhenMappingDoesNotExist()
    {
        var viewModel = CreateViewModel();

        await viewModel.LoadMappingForVendorAsync("Unknown Vendor");

        viewModel.VendorName.Should().Be("Unknown Vendor");
        viewModel.VariableName.Should().BeEmpty();
        viewModel.IsEditingExisting.Should().BeFalse();
    }

    [Fact]
    public async Task SaveCommand_ShouldInsertNewMapping_WhenVendorDoesNotExist()
    {
        var viewModel = CreateViewModel();
        viewModel.VendorName = "New Vendor";
        viewModel.VariableName = "Heat #";

        await viewModel.SaveCommand.ExecuteAsync(null);

        viewModel.StatusMessage.Should().Contain("Saved: New Vendor");
        viewModel.Mappings.Should().ContainSingle(mapping => mapping.VendorName == "New Vendor");
    }

    [Fact]
    public async Task DeleteSelectedCommand_ShouldRemoveSelectedMapping()
    {
        var viewModel = CreateViewModel();
        await viewModel.LoadMappingForVendorAsync("Skana");

        viewModel.SelectedMapping = viewModel.Mappings.First(mapping => mapping.VendorName == "Skana");
        await viewModel.DeleteSelectedCommand.ExecuteAsync(null);

        viewModel.Mappings.Should().NotContain(mapping => mapping.VendorName == "Skana");
    }

    private static ViewModel_Settings_Receiving_VendorVariables CreateViewModel()
    {
        var vendorService = new FakeVendorVariableService();
        var inforVisualService = new Mock<IService_InforVisual>();
        inforVisualService
            .Setup(service => service.FuzzySearchVendorsAsync(It.IsAny<string>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_FuzzySearchResult>
                    {
                        new() { Key = "Skana", Label = "Skana", Detail = "Vendor" }
                    }
                )
            );
        var errorHandler = new Mock<IService_ErrorHandler>().Object;
        var logger = new Mock<IService_LoggingUtility>().Object;
        var notification = new Mock<IService_Notification>().Object;

        return new ViewModel_Settings_Receiving_VendorVariables(
            vendorService,
            inforVisualService.Object,
            errorHandler,
            logger,
            notification
        );
    }

    private sealed class FakeVendorVariableService : IService_MySQL_ReceivingVendorVariable
    {
        private readonly List<Model_ReceivingVendorVariableMapping> _mappings =
        [
            new Model_ReceivingVendorVariableMapping { VendorName = "Skana", VariableName = "DM #" }
        ];

        public Task<Model_Dao_Result<List<Model_ReceivingVendorVariableMapping>>> GetAllMappingsAsync()
        {
            return Task.FromResult(Model_Dao_Result_Factory.Success(_mappings.ToList()));
        }

        public Task<Model_Dao_Result<Model_ReceivingVendorVariableMapping?>> GetMappingByVendorAsync(string vendorName)
        {
            var mapping = _mappings.FirstOrDefault(m =>
                string.Equals(m.VendorName, vendorName, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(Model_Dao_Result_Factory.Success<Model_ReceivingVendorVariableMapping?>(mapping));
        }

        public Task<Model_Dao_Result> SaveMappingAsync(string vendorName, string variableName)
        {
            var mapping = _mappings.FirstOrDefault(m =>
                string.Equals(m.VendorName, vendorName, StringComparison.OrdinalIgnoreCase));

            if (mapping is null)
            {
                _mappings.Add(new Model_ReceivingVendorVariableMapping
                {
                    VendorName = vendorName,
                    VariableName = variableName,
                });
            }
            else
            {
                mapping.VariableName = variableName;
            }

            return Task.FromResult(Model_Dao_Result_Factory.Success());
        }

        public Task<Model_Dao_Result> DeleteMappingAsync(string vendorName)
        {
            var mapping = _mappings.FirstOrDefault(m =>
                string.Equals(m.VendorName, vendorName, StringComparison.OrdinalIgnoreCase));
            if (mapping is not null)
            {
                _mappings.Remove(mapping);
            }

            return Task.FromResult(Model_Dao_Result_Factory.Success());
        }
    }

}
