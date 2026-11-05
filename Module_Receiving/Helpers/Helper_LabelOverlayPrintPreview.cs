using System;
using System.Drawing.Printing;
using System.Windows.Forms;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Helpers;

/// <summary>
/// What came back from the preview window: whether anything was sent, and the settings the operator
/// chose on the way, so they can be kept with this output.
/// </summary>
/// <param name="WasPrinted">True when the operator printed from the preview.</param>
/// <param name="Choice">The printer and settings used, or <c>null</c> when nothing was printed.</param>
/// <param name="Error">Why the print failed, when it did.</param>
public sealed record Model_LabelOverlayPreviewOutcome(
    bool WasPrinted,
    Model_LabelOverlayPrintDialogChoice? Choice,
    string? Error
);

/// <summary>
/// Shows the pages of a built print job before anything is sent, with a Print button that opens the print
/// dialog on this output's own settings.
/// </summary>
/// <remarks>
/// The preview renders the real document — the same pages, drawn by the same code — rather than a picture
/// of what the sheet is expected to look like, so a card sheet that spills past the printable area or a
/// marking that lands off the tag is visible before paper is spent. Printing happens from the preview so
/// the operator sees the job and the printer's settings in one place.
/// </remarks>
public static class Helper_LabelOverlayPrintPreview
{
    /// <summary>
    /// Shows the job and, if the operator prints, sends it.
    /// </summary>
    /// <param name="ownerHandle">The window the preview belongs to; <see cref="IntPtr.Zero"/> for none.</param>
    /// <param name="document">The job to show. Disposed by the caller.</param>
    /// <param name="settings">This output's setup, which the print dialog opens on.</param>
    public static Model_LabelOverlayPreviewOutcome Show(
        IntPtr ownerHandle,
        PrintDocument document,
        Model_LabelOverlayPrintSettings settings
    )
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(settings);

        Model_LabelOverlayPrintDialogChoice? choice = null;
        string? error = null;

        var preview = new PrintPreviewControl
        {
            Dock = DockStyle.Fill,
            Document = document,
            UseAntiAlias = true,
            AutoZoom = true,
            Rows = 1,
            Columns = 1,
        };

        using var form = new Form
        {
            Text = document.DocumentName,
            StartPosition = FormStartPosition.CenterParent,
            ShowInTaskbar = false,
            MinimizeBox = false,
            Width = 1000,
            Height = 820,
        };

        using var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 52,
            Padding = new Padding(12, 10, 12, 10),
        };

        using var closeButton = new Button { Text = "Close", Width = 110, Height = 30 };
        using var printButton = new Button { Text = "Print…", Width = 110, Height = 30 };

        closeButton.Click += (_, _) => form.Close();

        printButton.Click += (_, _) =>
        {
            var chosen = Helper_LabelOverlayPrintDialog.Show(form.Handle, settings);

            if (chosen is null)
            {
                return;
            }

            try
            {
                ApplyChoice(document, chosen);
                document.Print();

                choice = chosen;
                error = null;
                form.Close();
            }
            catch (Exception ex)
            {
                error = ex.Message;
                MessageBox.Show(
                    form,
                    $"The job could not be printed: {ex.Message}",
                    "Printing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        };

        buttons.Controls.Add(closeButton);
        buttons.Controls.Add(printButton);

        form.Controls.Add(preview);
        form.Controls.Add(buttons);

        var owner = ownerHandle == IntPtr.Zero ? null : new Win32Window(ownerHandle);
        _ = (owner is null ? form.ShowDialog() : form.ShowDialog(owner));

        preview.Document = null;

        return new Model_LabelOverlayPreviewOutcome(choice is not null, choice, error);
    }

    /// <summary>
    /// Puts what the print dialog came back with onto the job, so the pages already on screen are what is
    /// sent: the printer, its own settings, and how many copies to make.
    /// </summary>
    private static void ApplyChoice(
        PrintDocument document,
        Model_LabelOverlayPrintDialogChoice choice
    )
    {
        if (!string.IsNullOrWhiteSpace(choice.PrinterName))
        {
            document.PrinterSettings.PrinterName = choice.PrinterName;
        }

        Helper_PrinterDevMode.TryApply(document.PrinterSettings, choice.DevMode);
        document.PrinterSettings.Copies = (short)choice.Copies;
    }

    private sealed class Win32Window : IWin32Window
    {
        public Win32Window(IntPtr handle)
        {
            Handle = handle;
        }

        public IntPtr Handle { get; }
    }
}
