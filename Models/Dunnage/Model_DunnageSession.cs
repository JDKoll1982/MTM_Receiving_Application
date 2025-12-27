using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MTM_Receiving_Application.Models.Dunnage;

public class Model_DunnageSession : INotifyPropertyChanged
{
    private int _selectedTypeId;
    private string _selectedTypeName = string.Empty;
    private Model_DunnagePart? _selectedPart;
    private decimal _quantity;
    private string _poNumber = string.Empty;
    private string _location = string.Empty;
    private ObservableCollection<Model_DunnageLoad> _loads = new();

    public int SelectedTypeId
    {
        get => _selectedTypeId;
        set => SetField(ref _selectedTypeId, value);
    }

    public string SelectedTypeName
    {
        get => _selectedTypeName;
        set => SetField(ref _selectedTypeName, value);
    }

    public Model_DunnagePart? SelectedPart
    {
        get => _selectedPart;
        set => SetField(ref _selectedPart, value);
    }

    public decimal Quantity
    {
        get => _quantity;
        set => SetField(ref _quantity, value);
    }

    public string PONumber
    {
        get => _poNumber;
        set => SetField(ref _poNumber, value);
    }

    public string Location
    {
        get => _location;
        set => SetField(ref _location, value);
    }

    public ObservableCollection<Model_DunnageLoad> Loads
    {
        get => _loads;
        set
        {
            if (SetField(ref _loads, value))
            {
                OnPropertyChanged(nameof(HasLoads));
                _loads.CollectionChanged += (s, e) => OnPropertyChanged(nameof(HasLoads));
            }
        }
    }

    public bool HasLoads => Loads.Count > 0;

    public Model_DunnageSession()
    {
        _loads.CollectionChanged += (s, e) => OnPropertyChanged(nameof(HasLoads));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
