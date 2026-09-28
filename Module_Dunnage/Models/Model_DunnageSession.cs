using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_Dunnage.Models;

public class Model_DunnageSession : ObservableObject
{
    private int _selectedTypeId;
    private string _selectedTypeName = string.Empty;
    private Model_DunnagePart? _selectedPart;
    private int _numberOfLoads = 1;
    private decimal _quantity;
    private string _poNumber = string.Empty;
    private string _location = string.Empty;
    private string _inventoryMethod = string.Empty;
    private ObservableCollection<Model_DunnageLoad> _loads = new();
    private ObservableCollection<decimal> _loadQuantities = new();
    private readonly string?[] _udcValues = new string?[10];
    private Model_DunnageType? _selectedType;
    private bool _isPartSelectionFromImageSearch;

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

    public int NumberOfLoads
    {
        get => _numberOfLoads;
        set => SetProperty(ref _numberOfLoads, value);
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

    public string InventoryMethod
    {
        get => _inventoryMethod;
        set => SetProperty(ref _inventoryMethod, value);
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

    public ObservableCollection<decimal> LoadQuantities
    {
        get => _loadQuantities;
        set => SetProperty(ref _loadQuantities, value);
    }

    public string? Udc1 { get => _udcValues[0]; set => SetProperty(ref _udcValues[0], value); }
    public string? Udc2 { get => _udcValues[1]; set => SetProperty(ref _udcValues[1], value); }
    public string? Udc3 { get => _udcValues[2]; set => SetProperty(ref _udcValues[2], value); }
    public string? Udc4 { get => _udcValues[3]; set => SetProperty(ref _udcValues[3], value); }
    public string? Udc5 { get => _udcValues[4]; set => SetProperty(ref _udcValues[4], value); }
    public string? Udc6 { get => _udcValues[5]; set => SetProperty(ref _udcValues[5], value); }
    public string? Udc7 { get => _udcValues[6]; set => SetProperty(ref _udcValues[6], value); }
    public string? Udc8 { get => _udcValues[7]; set => SetProperty(ref _udcValues[7], value); }
    public string? Udc9 { get => _udcValues[8]; set => SetProperty(ref _udcValues[8], value); }
    public string? Udc10 { get => _udcValues[9]; set => SetProperty(ref _udcValues[9], value); }

    public string? GetUdcValue(int slot) =>
        slot >= 1 && slot <= 10 ? _udcValues[slot - 1] : null;

    public void SetUdcValue(int slot, string? value)
    {
        if (slot >= 1 && slot <= 10)
        {
            SetProperty(ref _udcValues[slot - 1], value);
        }
    }

    public Model_DunnageType? SelectedType
    {
        get => _selectedType;
        set => SetProperty(ref _selectedType, value);
    }

    public bool IsPartSelectionFromImageSearch
    {
        get => _isPartSelectionFromImageSearch;
        set => SetProperty(ref _isPartSelectionFromImageSearch, value);
    }

    public bool HasLoads => Loads.Count > 0;

    public Model_DunnageSession()
    {
        _loads.CollectionChanged += (s, e) => OnPropertyChanged(nameof(HasLoads));
        _loadQuantities.CollectionChanged += (s, e) => OnPropertyChanged(nameof(LoadQuantities));
    }
}
