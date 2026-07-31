using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_Shared.Contracts.Lookup;
using MTM_Receiving_Application.Module_Shared.Enums;
using MTM_Receiving_Application.Module_Shared.Models.Lookup;

namespace MTM_Receiving_Application.Module_Shared.Views.Controls;

/// <summary>
/// Reusable typed lookup textbox that executes the shared Infor Visual lookup workflow.
/// </summary>
public sealed partial class Control_Shared_TypedLookupTextBox : UserControl
{
    public static readonly DependencyProperty InputValueProperty = DependencyProperty.Register(
        nameof(InputValue),
        typeof(string),
        typeof(Control_Shared_TypedLookupTextBox),
        new PropertyMetadata(string.Empty)
    );

    public static readonly DependencyProperty LookupTypeProperty = DependencyProperty.Register(
        nameof(LookupType),
        typeof(Enum_SharedLookupType),
        typeof(Control_Shared_TypedLookupTextBox),
        new PropertyMetadata(Enum_SharedLookupType.PartNumber, OnLookupTypeChanged)
    );

    public static readonly DependencyProperty WarehouseCodeProperty = DependencyProperty.Register(
        nameof(WarehouseCode),
        typeof(string),
        typeof(Control_Shared_TypedLookupTextBox),
        new PropertyMetadata("002")
    );

    public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register(
        nameof(HeaderText),
        typeof(string),
        typeof(Control_Shared_TypedLookupTextBox),
        new PropertyMetadata(string.Empty)
    );

    public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
        nameof(PlaceholderText),
        typeof(string),
        typeof(Control_Shared_TypedLookupTextBox),
        new PropertyMetadata(string.Empty)
    );

    public static readonly DependencyProperty AutoValidateOnLostFocusProperty =
        DependencyProperty.Register(
            nameof(AutoValidateOnLostFocus),
            typeof(bool),
            typeof(Control_Shared_TypedLookupTextBox),
            new PropertyMetadata(true)
        );

    public static readonly DependencyProperty AutoResolveFuzzyMatchesProperty =
        DependencyProperty.Register(
            nameof(AutoResolveFuzzyMatches),
            typeof(bool),
            typeof(Control_Shared_TypedLookupTextBox),
            new PropertyMetadata(true)
        );

    public static readonly DependencyProperty PrefixPaddingRulesProperty = DependencyProperty.Register(
        nameof(PrefixPaddingRules),
        typeof(IReadOnlyList<Model_SharedLookupPrefixPaddingRule>),
        typeof(Control_Shared_TypedLookupTextBox),
        new PropertyMetadata(Array.Empty<Model_SharedLookupPrefixPaddingRule>())
    );

    public static readonly DependencyProperty RawInputProperty = DependencyProperty.Register(
        nameof(RawInput),
        typeof(string),
        typeof(Control_Shared_TypedLookupTextBox),
        new PropertyMetadata(string.Empty)
    );

    public static readonly DependencyProperty FormattedValueProperty = DependencyProperty.Register(
        nameof(FormattedValue),
        typeof(string),
        typeof(Control_Shared_TypedLookupTextBox),
        new PropertyMetadata(string.Empty)
    );

    public static readonly DependencyProperty ResolvedValueProperty = DependencyProperty.Register(
        nameof(ResolvedValue),
        typeof(string),
        typeof(Control_Shared_TypedLookupTextBox),
        new PropertyMetadata(string.Empty)
    );

    public static readonly DependencyProperty IsValidProperty = DependencyProperty.Register(
        nameof(IsValid),
        typeof(bool),
        typeof(Control_Shared_TypedLookupTextBox),
        new PropertyMetadata(false)
    );

    public static readonly DependencyProperty HasExactMatchProperty = DependencyProperty.Register(
        nameof(HasExactMatch),
        typeof(bool),
        typeof(Control_Shared_TypedLookupTextBox),
        new PropertyMetadata(false)
    );

    public static readonly DependencyProperty UsedFuzzyFallbackProperty =
        DependencyProperty.Register(
            nameof(UsedFuzzyFallback),
            typeof(bool),
            typeof(Control_Shared_TypedLookupTextBox),
            new PropertyMetadata(false)
        );

    public static readonly DependencyProperty IsValidationInProgressProperty =
        DependencyProperty.Register(
            nameof(IsValidationInProgress),
            typeof(bool),
            typeof(Control_Shared_TypedLookupTextBox),
            new PropertyMetadata(false)
        );

    public Control_Shared_TypedLookupTextBox()
    {
        InitializeComponent();
        ApplyLookupDefaults();
    }

    public event EventHandler<Model_SharedLookupValidationCompletedEventArgs>? ValidationCompleted;

    public IService_SharedLookupWorkflow? LookupWorkflowService { get; set; }

    public Model_SharedLookupValidationResult? LastValidationResult { get; private set; }

    public string InputValue
    {
        get => (string)GetValue(InputValueProperty);
        set => SetValue(InputValueProperty, value);
    }

    public Enum_SharedLookupType LookupType
    {
        get => (Enum_SharedLookupType)GetValue(LookupTypeProperty);
        set => SetValue(LookupTypeProperty, value);
    }

    public string WarehouseCode
    {
        get => (string)GetValue(WarehouseCodeProperty);
        set => SetValue(WarehouseCodeProperty, value);
    }

    public string HeaderText
    {
        get => (string)GetValue(HeaderTextProperty);
        set => SetValue(HeaderTextProperty, value);
    }

    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public bool AutoValidateOnLostFocus
    {
        get => (bool)GetValue(AutoValidateOnLostFocusProperty);
        set => SetValue(AutoValidateOnLostFocusProperty, value);
    }

    public bool AutoResolveFuzzyMatches
    {
        get => (bool)GetValue(AutoResolveFuzzyMatchesProperty);
        set => SetValue(AutoResolveFuzzyMatchesProperty, value);
    }

    public IReadOnlyList<Model_SharedLookupPrefixPaddingRule> PrefixPaddingRules
    {
        get =>
            (IReadOnlyList<Model_SharedLookupPrefixPaddingRule>)
                GetValue(PrefixPaddingRulesProperty);
        set => SetValue(PrefixPaddingRulesProperty, value);
    }

    public string RawInput
    {
        get => (string)GetValue(RawInputProperty);
        set => SetValue(RawInputProperty, value);
    }

    public string FormattedValue
    {
        get => (string)GetValue(FormattedValueProperty);
        set => SetValue(FormattedValueProperty, value);
    }

    public string ResolvedValue
    {
        get => (string)GetValue(ResolvedValueProperty);
        set => SetValue(ResolvedValueProperty, value);
    }

    public bool IsValid
    {
        get => (bool)GetValue(IsValidProperty);
        set => SetValue(IsValidProperty, value);
    }

    public bool HasExactMatch
    {
        get => (bool)GetValue(HasExactMatchProperty);
        set => SetValue(HasExactMatchProperty, value);
    }

    public bool UsedFuzzyFallback
    {
        get => (bool)GetValue(UsedFuzzyFallbackProperty);
        set => SetValue(UsedFuzzyFallbackProperty, value);
    }

    public bool IsValidationInProgress
    {
        get => (bool)GetValue(IsValidationInProgressProperty);
        set => SetValue(IsValidationInProgressProperty, value);
    }

    public async Task ValidateAsync()
    {
        SyncInputFromTextBox();

        var workflow = LookupWorkflowService ?? ResolveWorkflowService();
        if (workflow is null)
        {
            ApplyResult(
                new Model_SharedLookupValidationResult
                {
                    LookupType = LookupType,
                    RawInput = InputValue,
                    FormattedValue = InputValue?.Trim() ?? string.Empty,
                    ResolvedValue = string.Empty,
                    IsValid = false,
                    Message = "Shared lookup workflow service is not available.",
                }
            );
            return;
        }

        try
        {
            IsValidationInProgress = true;
            var request = new Model_SharedLookupRequest
            {
                LookupType = LookupType,
                RawInput = InputValue,
                WarehouseCode = WarehouseCode,
                PrefixPaddingRules = PrefixPaddingRules,
                AutoResolveFuzzyMatches = AutoResolveFuzzyMatches,
            };

            var result = await workflow.ValidateAsync(request);
            ApplyResult(result);
        }
        finally
        {
            IsValidationInProgress = false;
        }
    }

    private void SyncInputFromTextBox()
    {
        if (LookupTextBox is null)
        {
            return;
        }

        var liveText = LookupTextBox.Text ?? string.Empty;
        if (!string.Equals(InputValue, liveText, StringComparison.Ordinal))
        {
            InputValue = liveText;
        }
    }

    private void ApplyResult(Model_SharedLookupValidationResult result)
    {
        LastValidationResult = result;

        RawInput = result.RawInput;
        FormattedValue = result.FormattedValue;
        ResolvedValue = result.ResolvedValue;
        IsValid = result.IsValid;
        HasExactMatch = result.HasExactMatch;
        UsedFuzzyFallback = result.UsedFuzzyFallback;

        if (result.IsValid && string.IsNullOrWhiteSpace(result.ResolvedValue) is false)
        {
            InputValue = result.ResolvedValue;
        }
        else if (
            result.WasFormatted
            && string.IsNullOrWhiteSpace(result.FormattedValue) is false
        )
        {
            InputValue = result.FormattedValue;
        }

        HeaderTextBlock.Visibility = string.IsNullOrWhiteSpace(HeaderText)
            ? Visibility.Collapsed
            : Visibility.Visible;

        ValidationCompleted?.Invoke(this, new Model_SharedLookupValidationCompletedEventArgs(result));
    }

    private IService_SharedLookupWorkflow? ResolveWorkflowService()
    {
#pragma warning disable CS0618
        try
        {
            return App.GetService<IService_SharedLookupWorkflow>();
        }
        catch
        {
            return null;
        }
#pragma warning restore CS0618
    }

    private static void OnLookupTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Control_Shared_TypedLookupTextBox control)
        {
            control.ApplyLookupDefaults();
        }
    }

    private void ApplyLookupDefaults()
    {
        if (
            string.IsNullOrWhiteSpace(HeaderText)
            && ReadLocalValue(HeaderTextProperty) == DependencyProperty.UnsetValue
        )
        {
            HeaderText = LookupType == Enum_SharedLookupType.PartNumber ? "Part Number" : "Location";
        }

        if (
            string.IsNullOrWhiteSpace(PlaceholderText)
            && ReadLocalValue(PlaceholderTextProperty) == DependencyProperty.UnsetValue
        )
        {
            PlaceholderText = LookupType == Enum_SharedLookupType.PartNumber
                ? "Enter part number"
                : "Enter location";
        }

        if (HeaderTextBlock is not null)
        {
            HeaderTextBlock.Visibility = string.IsNullOrWhiteSpace(HeaderText)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }
    }

    private async void LookupTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (!AutoValidateOnLostFocus)
        {
            return;
        }

        await ValidateAsync();
    }

    private async void LookupTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != Windows.System.VirtualKey.Enter)
        {
            return;
        }

        e.Handled = true;
        await ValidateAsync();
    }

    private void LookupTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox)
        {
            return;
        }

        _ = textBox.DispatcherQueue.TryEnqueue(() => textBox.SelectAll());
    }
}
