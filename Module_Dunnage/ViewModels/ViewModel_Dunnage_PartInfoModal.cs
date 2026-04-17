using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.ViewModels;

/// <summary>
/// ViewModel for the Dunnage part information modal.
/// </summary>
public partial class ViewModel_Dunnage_PartInfoModal : ViewModel_Shared_Base
{
    private readonly IService_MySQL_Dunnage _dunnageService;

    public ViewModel_Dunnage_PartInfoModal(
        IService_MySQL_Dunnage dunnageService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _dunnageService = dunnageService;
    }

    [ObservableProperty]
    private string _heading = "Dunnage Part Details";

    [ObservableProperty]
    private string _partId = string.Empty;

    [ObservableProperty]
    private string _dunnageTypeName = string.Empty;

    [ObservableProperty]
    private string _homeLocation = string.Empty;

    [ObservableProperty]
    private string _inventoryType = "Not inventoried";

    [ObservableProperty]
    private string _loadType = "Not configured";

    [ObservableProperty]
    private string _imagePath = string.Empty;

    [ObservableProperty]
    private string _createdBy = string.Empty;

    [ObservableProperty]
    private string _createdDate = string.Empty;

    [ObservableProperty]
    private string _modifiedBy = string.Empty;

    [ObservableProperty]
    private string _modifiedDate = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Model_DunnageDisplayField> _specValues = new();

    [ObservableProperty]
    private ImageSource? _imageSource;

    public bool HasImage => ImageSource is not null;

    public async Task InitializeAsync(Model_DunnagePart part)
    {
        ArgumentNullException.ThrowIfNull(part);

        try
        {
            IsBusy = true;
            Heading = $"Part Details - {part.PartId}";
            PartId = part.PartId;
            DunnageTypeName = part.DunnageTypeName;
            HomeLocation = part.HomeLocation;
            ImagePath = part.ImagePath ?? string.Empty;
            ImageSource = part.ImageSource;
            OnPropertyChanged(nameof(HasImage));
            CreatedBy = part.CreatedBy;
            CreatedDate = part.CreatedDate.ToString("g", CultureInfo.CurrentCulture);
            ModifiedBy = string.IsNullOrWhiteSpace(part.ModifiedBy)
                ? "Never modified"
                : part.ModifiedBy;
            ModifiedDate = part.ModifiedDate.HasValue
                ? part.ModifiedDate.Value.ToString("g", CultureInfo.CurrentCulture)
                : "Never modified";
            LoadType = "Not configured";

            var inventoryResult = await _dunnageService.GetInventoryDetailsAsync(part.PartId);
            if (inventoryResult.IsSuccess && inventoryResult.Data is not null)
            {
                InventoryType = string.IsNullOrWhiteSpace(inventoryResult.Data.InventoryMethod)
                    ? "Not inventoried"
                    : inventoryResult.Data.InventoryMethod!;
            }
            else
            {
                InventoryType = "Not inventoried";
            }

            var specFields = part
                .SpecValuesDict.OrderBy(static pair => pair.Key)
                .Select(pair => new Model_DunnageDisplayField
                {
                    Label = pair.Key,
                    Value = pair.Value?.ToString() ?? string.Empty,
                })
                .ToList();

            if (string.IsNullOrWhiteSpace(ImagePath) is false)
            {
                specFields.Insert(
                    0,
                    new Model_DunnageDisplayField { Label = "image_path", Value = ImagePath }
                );
            }

            SpecValues = new ObservableCollection<Model_DunnageDisplayField>(specFields);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(InitializeAsync),
                nameof(ViewModel_Dunnage_PartInfoModal)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }
}
