using System;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Shared.Helpers;

namespace MTM_Receiving_Application.Module_Shared.Views.Controls;

/// <summary>
/// Quantity textbox shared by every module that asks for a count. It accepts whole numbers only
/// and lets the operator type simple arithmetic (<c>12*4</c>, <c>(8+4)/2</c>) that is reduced to
/// the answer when they press <c>=</c>, Enter, or leave the box.
/// </summary>
/// <remarks>
/// The trailing glyph tells the operator how the current text will be read: a checkmark when the
/// text is already a whole number the box can accept, and a warning when the entry is not a
/// quantity yet or an answer had to be rounded. Pointing at the box shows a one-line summary of
/// the allowed input; Ctrl+I while that summary is visible opens the short help window.
/// </remarks>
public sealed partial class Control_Shared_MathQuantityTextBox : UserControl
{
    /// <summary>One-line summary shown when the operator points at the box.</summary>
    public const string InputToolTipText =
        "Whole numbers only, no negatives.\nMath like 12*4 or (8+4)/2 works.\nCtrl+I for help.";

    private const string AcceptedToolTip = "Whole number — ready to use.";
    private const string InvalidToolTip =
        "Not a quantity yet. Use whole numbers with + - * / and parentheses.";
    private const string NegativeToolTip = "Quantity cannot be negative.";

    /// <summary>Virtual-key code shared by '=' and '+' on a standard keyboard.</summary>
    private const int OemPlusKey = 187;

    private const string CheckmarkGlyph = "\uE73E";
    private const string WarningGlyph = "\uE7BA";

    private readonly ToolTip _inputToolTip;
    private readonly KeyEventHandler _rootKeyHandler;

    private UIElement? _rootKeyTarget;
    private bool _isTextToolTipOpen;
    private bool _isWritingText;

    public Control_Shared_MathQuantityTextBox()
    {
        InitializeComponent();

        _rootKeyHandler = OnRootKeyDown;
        _inputToolTip = new ToolTip { Content = InputToolTipText };
        _inputToolTip.Opened += OnInputToolTipOpened;
        _inputToolTip.Closed += OnInputToolTipClosed;
        ToolTipService.SetToolTip(InputTextBox, _inputToolTip);

        ApplyHeaderVisibility();
        ApplyAutomationId();
        RefreshState(Text);

        Unloaded += (_, _) => DetachRootKeyHandler();
    }

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(Control_Shared_MathQuantityTextBox),
        new PropertyMetadata(string.Empty, OnTextPropertyChanged)
    );

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(decimal),
        typeof(Control_Shared_MathQuantityTextBox),
        new PropertyMetadata(0m)
    );

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header),
        typeof(string),
        typeof(Control_Shared_MathQuantityTextBox),
        new PropertyMetadata(string.Empty, OnHeaderPropertyChanged)
    );

    public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
        nameof(PlaceholderText),
        typeof(string),
        typeof(Control_Shared_MathQuantityTextBox),
        new PropertyMetadata(string.Empty)
    );

    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
        nameof(Minimum),
        typeof(decimal),
        typeof(Control_Shared_MathQuantityTextBox),
        new PropertyMetadata(0m)
    );

    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        nameof(Maximum),
        typeof(decimal),
        typeof(Control_Shared_MathQuantityTextBox),
        new PropertyMetadata(decimal.MaxValue)
    );

    public static readonly DependencyProperty InputAutomationIdProperty =
        DependencyProperty.Register(
            nameof(InputAutomationId),
            typeof(string),
            typeof(Control_Shared_MathQuantityTextBox),
            new PropertyMetadata(string.Empty, OnInputAutomationIdPropertyChanged)
        );

    /// <summary>
    /// Raised when the box loses focus after reducing its text, so the host can re-validate
    /// whatever is bound to it. The text may be the reduced answer or, when the entry is not a
    /// quantity yet, what the operator typed.
    /// </summary>
    public event EventHandler? ValueCommitted;

    /// <summary>
    /// Raised when Enter or Tab should move focus to the next field. The host decides where the
    /// next field is; the control only reports the intent so table navigation keeps working.
    /// </summary>
    public event EventHandler? NavigationRequested;

    /// <summary>Raised whenever the displayed text changes so hosts can mirror it.</summary>
    public event EventHandler? TextChanged;

    /// <summary>Raw text in the box: either a whole number or a short expression.</summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Last whole-number answer the box accepted.</summary>
    public decimal Value
    {
        get => (decimal)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>Optional label shown above the box.</summary>
    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    /// <summary>Smallest accepted answer. Defaults to zero, which blocks negative quantities.</summary>
    public decimal Minimum
    {
        get => (decimal)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>Largest accepted answer. Defaults to no practical limit.</summary>
    public decimal Maximum
    {
        get => (decimal)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>Automation id applied to the inner textbox for UI automation.</summary>
    public string InputAutomationId
    {
        get => (string)GetValue(InputAutomationIdProperty);
        set => SetValue(InputAutomationIdProperty, value);
    }

    /// <summary>Moves focus into the textbox and selects the current text.</summary>
    public void FocusInputAndSelectAll()
    {
        InputTextBox?.Focus(FocusState.Programmatic);
        InputTextBox?.SelectAll();
    }

    /// <summary>
    /// Opens the quantity help window. Exposed so a host can offer the same help from its own
    /// affordance (menu item, header link) without duplicating the window.
    /// </summary>
    public static void ShowHelp()
    {
        _ = View_Shared_MathQuantityHelpWindow.ShowAsync(App.MainWindow);
    }

    private static void OnTextPropertyChanged(
        DependencyObject sender,
        DependencyPropertyChangedEventArgs args
    )
    {
        if (sender is not Control_Shared_MathQuantityTextBox control || control._isWritingText)
        {
            return;
        }

        control.RefreshState(args.NewValue as string ?? string.Empty);
    }

    private static void OnHeaderPropertyChanged(
        DependencyObject sender,
        DependencyPropertyChangedEventArgs args
    )
    {
        if (sender is Control_Shared_MathQuantityTextBox control)
        {
            control.ApplyHeaderVisibility();
        }
    }

    private static void OnInputAutomationIdPropertyChanged(
        DependencyObject sender,
        DependencyPropertyChangedEventArgs args
    )
    {
        if (sender is Control_Shared_MathQuantityTextBox control)
        {
            control.ApplyAutomationId();
        }
    }

    private void ApplyHeaderVisibility()
    {
        if (HeaderTextBlock is null)
        {
            return;
        }

        HeaderTextBlock.Visibility = string.IsNullOrWhiteSpace(Header)
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    private void ApplyAutomationId()
    {
        if (InputTextBox is null || string.IsNullOrWhiteSpace(InputAutomationId))
        {
            return;
        }

        AutomationProperties.SetAutomationId(InputTextBox, InputAutomationId);
    }

    private void InputTextBox_BeforeTextChanging(
        TextBox sender,
        TextBoxBeforeTextChangingEventArgs args
    )
    {
        if (args is null || args.NewText.Length == 0)
        {
            return;
        }

        // Filter as the operator types so letters, decimals, percent signs and thousands
        // separators can never reach the evaluator.
        foreach (var character in args.NewText)
        {
            if (Helper_SharedMathExpression.IsAllowedInputCharacter(character) is false)
            {
                args.Cancel = true;
                return;
            }
        }
    }

    private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isWritingText)
        {
            return;
        }

        RefreshState(InputTextBox?.Text ?? string.Empty);
        TextChanged?.Invoke(this, EventArgs.Empty);
    }

    private void InputTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        // Focus loss is the single place that reports a commit, so each cell edit validates the
        // row exactly once whether the operator clicked away, pressed Enter, or pressed Tab.
        Commit();
        ValueCommitted?.Invoke(this, EventArgs.Empty);
    }

    private void InputTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        // A short pause keeps the click from collapsing the selection the operator just made.
        _ = (InputTextBox?.DispatcherQueue)?.TryEnqueue(() => InputTextBox?.SelectAll());
    }

    private void InputTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            e.Handled = true;
            Commit();
            NavigationRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (e.Key == Windows.System.VirtualKey.Tab)
        {
            e.Handled = true;
            Commit();
            NavigationRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        // '=' shares its key with '+' on a standard keyboard: plain '=' accepts the answer,
        // Shift+'=' still types an addition sign.
        if (
            (int)e.Key == OemPlusKey
            && IsKeyDown(Windows.System.VirtualKey.Shift) is false
        )
        {
            e.Handled = true;
            Commit();
        }
    }

    private static bool IsKeyDown(Windows.System.VirtualKey key)
    {
        return InputKeyboardSource
            .GetKeyStateForCurrentThread(key)
            .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
    }

    /// <summary>
    /// While the summary is on screen, listen for Ctrl+I at the window root. Listening at the root
    /// (rather than only on the textbox) is what lets the shortcut work when the operator is
    /// merely pointing at the box and the keyboard focus sits somewhere else.
    /// </summary>
    private void OnInputToolTipOpened(object? sender, object e)
    {
        _isTextToolTipOpen = true;

        var root = XamlRoot?.Content;
        if (root is null || _rootKeyTarget is not null)
        {
            return;
        }

        _rootKeyTarget = root;
        root.AddHandler(UIElement.KeyDownEvent, _rootKeyHandler, handledEventsToo: true);
    }

    private void OnInputToolTipClosed(object? sender, object e)
    {
        _isTextToolTipOpen = false;
        DetachRootKeyHandler();
    }

    private void DetachRootKeyHandler()
    {
        if (_rootKeyTarget is null)
        {
            return;
        }

        _rootKeyTarget.RemoveHandler(UIElement.KeyDownEvent, _rootKeyHandler);
        _rootKeyTarget = null;
    }

    private void OnRootKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (_isTextToolTipOpen is false)
        {
            return;
        }

        if (
            e.Key != Windows.System.VirtualKey.I
            || IsKeyDown(Windows.System.VirtualKey.Control) is false
        )
        {
            return;
        }

        e.Handled = true;
        DetachRootKeyHandler();
        ShowHelp();
    }

    /// <summary>
    /// Reduces the current text to its answer when it is a valid quantity. Focus loss does the
    /// reporting, so this method only rewrites the text and updates the status glyph.
    /// </summary>
    private void Commit()
    {
        var raw = InputTextBox?.Text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(raw))
        {
            Value = 0m;
            RefreshState(raw);
            return;
        }

        if (TryResolveAnswer(raw, out var answer) is false)
        {
            // Keep the operator's text (and the warning glyph) so they can fix it in place.
            RefreshState(raw);
            return;
        }

        var answerText = Helper_SharedMathExpression.ToInputText(answer);
        Value = answer;
        WriteText(answerText);
        ShowState(CheckmarkGlyph, "SystemFillColorSuccessBrush", AcceptedToolTip);
    }

    /// <summary>
    /// Evaluates the typed text into a whole-number quantity, enforcing the minimum, maximum and
    /// the whole-number rule.
    /// </summary>
    private bool TryResolveAnswer(string raw, out decimal answer)
    {
        answer = 0m;

        if (
            Helper_SharedMathExpression.TryEvaluateWholeNumber(
                raw,
                out var wholeNumber,
                out _
            ) is false
        )
        {
            return false;
        }

        if (wholeNumber < Minimum)
        {
            return false;
        }

        if (Maximum != decimal.MaxValue && wholeNumber > Maximum)
        {
            return false;
        }

        answer = wholeNumber;
        return true;
    }

    private void WriteText(string text)
    {
        if (string.Equals(InputTextBox?.Text, text, StringComparison.Ordinal))
        {
            return;
        }

        _isWritingText = true;
        try
        {
            if (InputTextBox is not null)
            {
                InputTextBox.Text = text;
            }

            Text = text;
        }
        finally
        {
            _isWritingText = false;
        }

        TextChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Re-evaluates the text and updates the trailing glyph: a checkmark when the text is already
    /// an acceptable whole number, otherwise a warning whose tooltip explains what to fix.
    /// </summary>
    private void RefreshState(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            StateIcon.Visibility = Visibility.Collapsed;
            ToolTipService.SetToolTip(StateIcon, null);
            return;
        }

        if (Helper_SharedMathExpression.TryEvaluate(raw, out var exact) is false)
        {
            ShowState(WarningGlyph, "SystemFillColorCautionBrush", InvalidToolTip);
            return;
        }

        if (exact < 0 || exact < Minimum)
        {
            ShowState(WarningGlyph, "SystemFillColorCautionBrush", NegativeToolTip);
            return;
        }

        var wholeNumber = Math.Round(exact, 0, MidpointRounding.AwayFromZero);

        if (Maximum != decimal.MaxValue && wholeNumber > Maximum)
        {
            ShowState(
                WarningGlyph,
                "SystemFillColorCautionBrush",
                $"Quantity cannot be more than {Helper_SharedMathExpression.ToInputText(Maximum)}."
            );
            return;
        }

        if (wholeNumber != exact)
        {
            ShowState(
                WarningGlyph,
                "SystemFillColorCautionBrush",
                $"Rounds to {Helper_SharedMathExpression.ToInputText(wholeNumber)} — whole numbers only."
            );
            return;
        }

        Value = wholeNumber;
        ShowState(CheckmarkGlyph, "SystemFillColorSuccessBrush", AcceptedToolTip);
    }

    private void ShowState(string glyph, string brushResourceKey, string toolTip)
    {
        if (StateIcon is null)
        {
            return;
        }

        StateIcon.Glyph = glyph;
        StateIcon.Foreground = ResolveStateBrush(brushResourceKey);
        StateIcon.Visibility = Visibility.Visible;
        ToolTipService.SetToolTip(StateIcon, toolTip);
    }

    /// <summary>
    /// Resolves a status brush from the theme, falling back to the same colors the app uses for
    /// success and caution so the glyph never renders in the default text color.
    /// </summary>
    private static Brush ResolveStateBrush(string resourceKey)
    {
        if (
            Application.Current?.Resources.TryGetValue(resourceKey, out var value) is true
            && value is Brush themeBrush
        )
        {
            return themeBrush;
        }

        var fallback =
            resourceKey == "SystemFillColorSuccessBrush"
                ? Windows.UI.Color.FromArgb(255, 16, 137, 62)
                : Windows.UI.Color.FromArgb(255, 157, 93, 0);

        return new SolidColorBrush(fallback);
    }
}
