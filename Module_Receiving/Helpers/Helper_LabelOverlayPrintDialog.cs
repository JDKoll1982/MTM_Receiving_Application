using System;
using System.Linq;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Helpers;

/// <summary>
/// What the operator chose in the print dialog: which printer, that printer's settings, and how many
/// copies to make.
/// </summary>
/// <param name="PrinterName">The printer the job goes to.</param>
/// <param name="DevMode">The printer's settings as the dialog left them, or <c>null</c> for its defaults.</param>
/// <param name="Copies">How many copies of each page to make.</param>
public sealed record Model_LabelOverlayPrintDialogChoice(
    string PrinterName,
    byte[]? DevMode,
    int Copies
);

/// <summary>
/// The print dialog the overlay opens when Print is clicked, seeded with the settings the current output
/// was set up with.
/// </summary>
/// <remarks>
/// The dialog is the printer's own, so it starts on the tray, media, duplex and quality this output was
/// last set up for, and the operator can change any of them — or pick another printer — without leaving
/// the screen. Whatever comes back is kept with this output and re-applied to later jobs; the printer's
/// stored defaults are never written, so a choice made here cannot leak into another program.
/// </remarks>
public static class Helper_LabelOverlayPrintDialog
{
    /// <summary>
    /// Shows the dialog for this output.
    /// </summary>
    /// <param name="ownerHandle">The window the dialog belongs to; <see cref="IntPtr.Zero"/> for none.</param>
    /// <param name="settings">The setup the dialog opens on.</param>
    /// <returns>The choice, or <c>null</c> when the operator cancelled or no printer is installed.</returns>
    public static Model_LabelOverlayPrintDialogChoice? Show(
        IntPtr ownerHandle,
        Model_LabelOverlayPrintSettings settings
    )
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (!AnyPrinterInstalled())
        {
            return null;
        }

        var printerSettings = new System.Drawing.Printing.PrinterSettings();

        if (!string.IsNullOrWhiteSpace(settings.PrinterName))
        {
            printerSettings.PrinterName = settings.PrinterName.Trim();
        }

        if (!printerSettings.IsValid)
        {
            return null;
        }

        // The dialog opens on this output's own settings, not on whatever the printer last printed.
        Helper_PrinterDevMode.TryApply(printerSettings, settings.GetDevMode());

        printerSettings.Copies = (short)settings.ResolveCopies();

        using var dialog = new System.Windows.Forms.PrintDialog
        {
            PrinterSettings = printerSettings,
            UseEXDialog = true,
            AllowCurrentPage = false,
            AllowSelection = false,
            AllowPrintToFile = false,

            // The overlay prints every page it builds, so a page range has nothing to choose between.
            AllowSomePages = false,
            ShowHelp = false,
            ShowNetwork = true,
        };

        var owner = ownerHandle == IntPtr.Zero ? null : new Win32Window(ownerHandle);
        var result = owner is null ? dialog.ShowDialog() : dialog.ShowDialog(owner);

        if (result != System.Windows.Forms.DialogResult.OK)
        {
            return null;
        }

        var chosen = dialog.PrinterSettings;

        return new Model_LabelOverlayPrintDialogChoice(
            chosen.PrinterName ?? string.Empty,
            Helper_PrinterDevMode.ReadDevMode(chosen),
            Math.Clamp((int)chosen.Copies, 1, Model_LabelOverlayPrintSettings.MaximumCopies)
        );
    }

    private static bool AnyPrinterInstalled()
    {
        try
        {
            return System.Drawing.Printing.PrinterSettings
                .InstalledPrinters.Cast<string>()
                .Any(name => !string.IsNullOrWhiteSpace(name));
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            // A print spooler that is stopped reports no printers rather than a list.
            return false;
        }
    }

    private sealed class Win32Window : System.Windows.Forms.IWin32Window
    {
        public Win32Window(IntPtr handle)
        {
            Handle = handle;
        }

        public IntPtr Handle { get; }
    }
}
