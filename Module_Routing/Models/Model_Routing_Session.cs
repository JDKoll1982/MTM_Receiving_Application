using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_Routing.Models;

/// <summary>
/// Model representing the current routing session state
/// Manages the queue of labels being created and the next label number
/// </summary>
public partial class Model_Routing_Session : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Model_Routing_Label> _labelQueue = new();

    [ObservableProperty]
    private int _nextLabelNumber = 1;

    /// <summary>
    /// Gets the total count of labels in the current queue
    /// </summary>
    public int QueueCount => LabelQueue?.Count ?? 0;
}
