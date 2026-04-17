using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_Dunnage.ViewModels;

public partial class ViewModel_Dunnage_PartDialogValidation : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    private string _partId = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    private int _typeId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    private string _inventoryType = string.Empty;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    public bool IsValid =>
        string.IsNullOrWhiteSpace(PartId) is false
        && TypeId > 0
        && string.IsNullOrWhiteSpace(InventoryType) is false;

    public void Update(string? partId, int typeId, string? inventoryType)
    {
        PartId = partId?.Trim() ?? string.Empty;
        TypeId = typeId;
        InventoryType = inventoryType?.Trim() ?? string.Empty;
        RefreshValidationMessage();
    }

    public bool ValidateForSubmit()
    {
        RefreshValidationMessage();
        return IsValid;
    }

    private void RefreshValidationMessage()
    {
        if (string.IsNullOrWhiteSpace(PartId))
        {
            ValidationMessage = "Part ID is required.";
            return;
        }

        if (TypeId <= 0)
        {
            ValidationMessage = "A dunnage type is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(InventoryType))
        {
            ValidationMessage = "Inventory Type is required.";
            return;
        }

        ValidationMessage = string.Empty;
    }
}
