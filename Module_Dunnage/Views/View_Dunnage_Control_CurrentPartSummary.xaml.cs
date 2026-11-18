using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

/// <summary>
/// Summary card for the non-inventoried part currently being processed: type, part, home
/// location, quantity type, the running load count and the running quantity total.
/// </summary>
public sealed partial class View_Dunnage_Control_CurrentPartSummary : UserControl
{
    public static readonly DependencyProperty TypeNameProperty = DependencyProperty.Register(
        nameof(TypeName),
        typeof(string),
        typeof(View_Dunnage_Control_CurrentPartSummary),
        new PropertyMetadata(string.Empty)
    );

    public static readonly DependencyProperty PartIdProperty = DependencyProperty.Register(
        nameof(PartId),
        typeof(string),
        typeof(View_Dunnage_Control_CurrentPartSummary),
        new PropertyMetadata(string.Empty)
    );

    public static readonly DependencyProperty HomeLocationDisplayProperty =
        DependencyProperty.Register(
            nameof(HomeLocationDisplay),
            typeof(string),
            typeof(View_Dunnage_Control_CurrentPartSummary),
            new PropertyMetadata(string.Empty)
        );

    public static readonly DependencyProperty QuantityTypeProperty = DependencyProperty.Register(
        nameof(QuantityType),
        typeof(string),
        typeof(View_Dunnage_Control_CurrentPartSummary),
        new PropertyMetadata("Quantity")
    );

    public static readonly DependencyProperty VisualSourceProperty = DependencyProperty.Register(
        nameof(VisualSource),
        typeof(ImageSource),
        typeof(View_Dunnage_Control_CurrentPartSummary),
        new PropertyMetadata(null, OnVisualSourceChanged)
    );

    public static readonly DependencyProperty HasVisualProperty = DependencyProperty.Register(
        nameof(HasVisual),
        typeof(bool),
        typeof(View_Dunnage_Control_CurrentPartSummary),
        new PropertyMetadata(false)
    );

    public static readonly DependencyProperty LoadCountProperty = DependencyProperty.Register(
        nameof(LoadCount),
        typeof(int),
        typeof(View_Dunnage_Control_CurrentPartSummary),
        new PropertyMetadata(0)
    );

    public static readonly DependencyProperty CurrentTotalProperty = DependencyProperty.Register(
        nameof(CurrentTotal),
        typeof(string),
        typeof(View_Dunnage_Control_CurrentPartSummary),
        new PropertyMetadata("0")
    );

    /// <summary>
    /// Raised when the summary image is tapped so the host can open the full-size image.
    /// </summary>
    public event EventHandler? ImageTapped;

    public View_Dunnage_Control_CurrentPartSummary()
    {
        InitializeComponent();
    }

    public string TypeName
    {
        get => (string)GetValue(TypeNameProperty);
        set => SetValue(TypeNameProperty, value);
    }

    public string PartId
    {
        get => (string)GetValue(PartIdProperty);
        set => SetValue(PartIdProperty, value);
    }

    public string HomeLocationDisplay
    {
        get => (string)GetValue(HomeLocationDisplayProperty);
        set => SetValue(HomeLocationDisplayProperty, value);
    }

    public string QuantityType
    {
        get => (string)GetValue(QuantityTypeProperty);
        set => SetValue(QuantityTypeProperty, value);
    }

    public ImageSource? VisualSource
    {
        get => (ImageSource?)GetValue(VisualSourceProperty);
        set => SetValue(VisualSourceProperty, value);
    }

    public bool HasVisual
    {
        get => (bool)GetValue(HasVisualProperty);
        set => SetValue(HasVisualProperty, value);
    }

    public int LoadCount
    {
        get => (int)GetValue(LoadCountProperty);
        set => SetValue(LoadCountProperty, value);
    }

    public string CurrentTotal
    {
        get => (string)GetValue(CurrentTotalProperty);
        set => SetValue(CurrentTotalProperty, value);
    }

    private static void OnVisualSourceChanged(
        DependencyObject sender,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (sender is View_Dunnage_Control_CurrentPartSummary summary)
        {
            summary.HasVisual = e.NewValue is ImageSource;
        }
    }

    private void OnImageTapped(object sender, TappedRoutedEventArgs e)
    {
        ImageTapped?.Invoke(this, EventArgs.Empty);
    }
}
