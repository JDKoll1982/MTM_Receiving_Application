using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Module_Volvo.ViewModels;

public partial class ViewModel_Volvo_PartNumberEditDialog : ObservableObject
{
    private List<Model_VolvoPart> _allParts = new();
    private HashSet<string> _existingPartNumbers = new(StringComparer.OrdinalIgnoreCase);

    [ObservableProperty]
    private string _currentPartNumber = string.Empty;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Model_VolvoPart> _filteredParts = new();

    [ObservableProperty]
    private Model_VolvoPart? _selectedPart;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public bool HasErrorMessage => !string.IsNullOrWhiteSpace(ErrorMessage);

    public void Initialize(Model_VolvoPartNumberEditDialog model)
    {
        CurrentPartNumber = model.CurrentPartNumber;
        _allParts = model
            .AvailableParts.OrderBy(part => part.PartNumber, StringComparer.OrdinalIgnoreCase)
            .ToList();
        _existingPartNumbers = model.ExistingPartNumbers;
        FilteredParts = new ObservableCollection<Model_VolvoPart>(_allParts);
        SelectedPart = _allParts.FirstOrDefault(part =>
            part.PartNumber.Equals(CurrentPartNumber, StringComparison.OrdinalIgnoreCase)
        );
    }

    public bool TryValidateSelection(out string errorMessage)
    {
        if (SelectedPart == null)
        {
            errorMessage = "Please select a replacement part number.";
            ErrorMessage = errorMessage;
            return false;
        }

        if (
            _existingPartNumbers.Contains(SelectedPart.PartNumber)
            && !SelectedPart.PartNumber.Equals(
                CurrentPartNumber,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            errorMessage =
                $"Part {SelectedPart.PartNumber} already exists in the current list. Remove or edit the existing card instead.";
            ErrorMessage = errorMessage;
            return false;
        }

        ErrorMessage = string.Empty;
        errorMessage = string.Empty;
        return true;
    }

    partial void OnSearchTextChanged(string value)
    {
        var trimmedValue = value?.Trim() ?? string.Empty;
        var filtered = string.IsNullOrWhiteSpace(trimmedValue)
            ? _allParts
            : _allParts
                .Where(part =>
                    part.PartNumber.Contains(trimmedValue, StringComparison.OrdinalIgnoreCase)
                )
                .ToList();

        FilteredParts = new ObservableCollection<Model_VolvoPart>(filtered);

        if (
            SelectedPart != null
            && !FilteredParts.Any(part => part.PartNumber == SelectedPart.PartNumber)
        )
        {
            SelectedPart = null;
        }
    }

    partial void OnErrorMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasErrorMessage));
    }
}
