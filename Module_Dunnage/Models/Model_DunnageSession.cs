using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_Dunnage.Models;

public class Model_DunnageSession : ObservableObject
{
    private int _selectedTypeId;
    private string _selectedTypeName = string.Empty;
    private Model_DunnagePart? _selectedPart;
    private decimal _quantity;
    private string _poNumber = string.Empty;
    private string _location = string.Empty;
    private ObservableCollection<Model_DunnageLoad> _loads = new();
    private System.Collections.Generic.Dictionary<string, object>? _specValues;
    private Model_DunnageType? _selectedType;

    public int SelectedTypeId
    {
        get => _selectedTypeId;
        set => SetProperty(ref _selectedTypeId, value);
    }

    public string SelectedTypeName
    {
        get => _selectedTypeName;
        set => SetProperty(ref _selectedTypeName, value);
    }

    public Model_DunnagePart? SelectedPart
    {
        get => _selectedPart;
        set => SetProperty(ref _selectedPart, value);
    }

    public decimal Quantity
    {
        get => _quantity;
        set => SetProperty(ref _quantity, value);
    }

    public string PONumber
    {
        get => _poNumber;
        set => SetProperty(ref _poNumber, value);
    }

    public string Location
    {
        get => _location;
        set => SetProperty(ref _location, value);
    }

    public ObservableCollection<Model_DunnageLoad> Loads
    {
        get => _loads;
        set
        {
            if (SetProperty(ref _loads, value))
            {
                OnPropertyChanged(nameof(HasLoads));
                _loads.CollectionChanged += (s, e) => OnPropertyChanged(nameof(HasLoads));
            }
        }
    }

    public System.Collections.Generic.Dictionary<string, object>? SpecValues
    {
        get => _specValues;
        set => SetProperty(ref _specValues, value);
    }

    public Model_DunnageType? SelectedType
    {
        get => _selectedType;
        set => SetProperty(ref _selectedType, value);
    }

    public bool HasLoads => Loads.Count > 0;

    public Model_DunnageSession()
    {
        _loads.CollectionChanged += (s, e) => OnPropertyChanged(nameof(HasLoads));
    }
}
