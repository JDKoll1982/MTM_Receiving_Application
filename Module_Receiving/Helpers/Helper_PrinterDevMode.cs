using System;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Runtime.InteropServices;

namespace MTM_Receiving_Application.Module_Receiving.Helpers;

/// <summary>
/// Reads, edits and re-applies a printer's <c>DEVMODE</c> for one print job, without ever changing the
/// printer's stored defaults.
/// </summary>
/// <remarks>
/// A <c>DEVMODE</c> block holds the per-job choices — tray, media, duplex, quality — that a driver would
/// otherwise take from the printer's saved defaults. Everything here works on a private copy:
/// <see cref="PrinterSettings.GetHdevmode"/> hands one out, <c>DocumentProperties</c> edits it, and
/// <see cref="PrinterSettings.SetHdevmode"/> applies it to a single <see cref="PrintDocument"/>.
/// The printer's own defaults are only ever written by <c>SetPrinter</c>, which is deliberately not called
/// anywhere in this file, so a tray chosen here cannot leak into another program.
/// A <c>DEVMODE</c> is device specific, so a saved copy is only re-applied to the printer it came from.
/// </remarks>
public static class Helper_PrinterDevMode
{
    private const int DmInBuffer = 0x00000008;
    private const int DmOutBuffer = 0x00000002;
    private const int DmInPrompt = 0x00000004;
    private const int IdOk = 1;

    private const uint GmemMoveable = 0x0002;

    /// <summary>
    /// Shows the printer driver's own dialog for this printer and returns the chosen settings as a
    /// <c>DEVMODE</c> block, or <c>null</c> when the operator cancels.
    /// </summary>
    /// <param name="printerName">The printer whose dialog to show.</param>
    /// <param name="ownerHandle">The window that owns the dialog.</param>
    /// <param name="existing">
    /// A block previously returned by this method, re-opened for editing so a second visit continues from
    /// the first. Ignored when it did not come from this printer.
    /// </param>
    public static byte[]? EditForJob(string printerName, IntPtr ownerHandle, byte[]? existing = null)
    {
        if (string.IsNullOrWhiteSpace(printerName))
        {
            return null;
        }

        var name = printerName.Trim();

        if (!OpenPrinter(name, out var printerHandle, IntPtr.Zero))
        {
            throw new Win32Exception(
                Marshal.GetLastWin32Error(),
                $"The printer '{name}' could not be opened for setup."
            );
        }

        var modeHandle = IntPtr.Zero;
        var output = IntPtr.Zero;

        try
        {
            var settings = new PrinterSettings { PrinterName = name };

            // The printer's current settings, as a starting point; a saved block replaces it so a second
            // visit to the dialog carries on from the operator's own choices.
            modeHandle = settings.GetHdevmode();

            if (existing is { Length: > 0 })
            {
                var saved = ToGlobalHandle(existing);

                if (saved != IntPtr.Zero)
                {
                    if (modeHandle != IntPtr.Zero)
                    {
                        GlobalFree(modeHandle);
                    }

                    modeHandle = saved;
                }
            }

            var size = DocumentProperties(
                ownerHandle,
                printerHandle,
                name,
                IntPtr.Zero,
                modeHandle,
                0
            );

            if (size <= 0)
            {
                throw new Win32Exception(
                    Marshal.GetLastWin32Error(),
                    $"The printer '{name}' did not report how much room its settings need."
                );
            }

            output = Marshal.AllocHGlobal(size);

            try
            {
                // Ask the driver for its own dialog, seeded with the settings above, and keep the result
                // in our buffer. DM_OUT_BUFFER writes to `output`; nothing is written back to the printer.
                var result = DocumentProperties(
                    ownerHandle,
                    printerHandle,
                    name,
                    output,
                    modeHandle,
                    DmInPrompt | DmInBuffer | DmOutBuffer
                );

                if (result != IdOk)
                {
                    return null;
                }

                var chosen = new byte[size];
                Marshal.Copy(output, chosen, 0, size);

                return chosen;
            }
            finally
            {
                Marshal.FreeHGlobal(output);
            }
        }
        finally
        {
            if (modeHandle != IntPtr.Zero)
            {
                GlobalFree(modeHandle);
            }

            ClosePrinter(printerHandle);
        }
    }

    /// <summary>
    /// Applies a saved <c>DEVMODE</c> block to a document's printer settings.
    /// </summary>
    /// <returns><c>true</c> when the block was applied; <c>false</c> when it was empty or unusable, in
    /// which case the document keeps the printer's current settings.</returns>
    /// <remarks>
    /// A block saved against a different printer is device specific and cannot be applied, so it is
    /// refused rather than forced — the job then prints with the printer's own settings instead of failing.
    /// </remarks>
    public static bool TryApply(PrinterSettings printerSettings, byte[]? devMode)
    {
        ArgumentNullException.ThrowIfNull(printerSettings);

        if (devMode is not { Length: > 0 } || !printerSettings.IsValid)
        {
            return false;
        }

        var handle = ToGlobalHandle(devMode);
        if (handle == IntPtr.Zero)
        {
            return false;
        }

        try
        {
            printerSettings.SetHdevmode(handle);
            return true;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            // A block from a printer that has since been replaced cannot be applied; keep the job going.
            return false;
        }
        finally
        {
            GlobalFree(handle);
        }
    }

    /// <summary>
    /// Reads the settings a printer's current job would use as a <c>DEVMODE</c> block, so a dialog that
    /// changed them can be saved back and re-applied to later jobs.
    /// </summary>
    /// <returns>The block, or <c>null</c> when the printer is unusable or its settings cannot be read.</returns>
    public static byte[]? ReadDevMode(PrinterSettings printerSettings)
    {
        ArgumentNullException.ThrowIfNull(printerSettings);

        if (!printerSettings.IsValid)
        {
            return null;
        }

        var handle = printerSettings.GetHdevmode();

        if (handle == IntPtr.Zero)
        {
            return null;
        }

        try
        {
            var target = GlobalLock(handle);

            if (target == IntPtr.Zero)
            {
                return null;
            }

            try
            {
                var mode = Marshal.PtrToStructure<Model_DevModePublic>(target);

                // The public part is followed by the driver's own private tail, whose length the driver
                // reports; copying both is what keeps the block valid for the driver it came from.
                var size = mode.Size + mode.DriverExtra;

                if (size <= 0 || size > 64 * 1024)
                {
                    return null;
                }

                var bytes = new byte[size];
                Marshal.Copy(target, bytes, 0, size);

                return bytes;
            }
            finally
            {
                GlobalUnlock(handle);
            }
        }
        finally
        {
            GlobalFree(handle);
        }
    }

    /// <summary>
    /// The paper source the block names, or <c>null</c> when the block does not name one.
    /// </summary>
    /// <remarks>
    /// The tray is the setting operators change most often, and several drivers ignore it when the block
    /// is handed back to them, so it is read here and applied to the job through the page settings
    /// instead of relying on the driver to remember it.
    /// </remarks>
    public static short? ReadPaperSource(byte[]? devMode)
    {
        if (devMode is not { Length: > 0 })
        {
            return null;
        }

        try
        {
            var mode = FromBytes(devMode);

            return (mode.Fields & DmDefaultSource) != 0 ? mode.DefaultSource : null;
        }
        catch (Exception)
        {
            // Only ever used to seed a picker, so a block that cannot be read simply names no tray.
            return null;
        }
    }

    /// <summary>
    /// A plain reading of the settings a block holds, for the log: which tray, which media, which way up.
    /// </summary>
    /// <remarks>
    /// Only the public part of the structure is read, and only fields the driver flagged as meaningful.
    /// The driver-specific part is not interpreted, because its layout belongs to the driver.
    /// </remarks>
    public static string Describe(byte[]? devMode)
    {
        if (devMode is not { Length: > 0 })
        {
            return "printer defaults";
        }

        try
        {
            var mode = FromBytes(devMode);

            var parts = new System.Collections.Generic.List<string>();

            if ((mode.Fields & DmOrientation) != 0)
            {
                parts.Add(mode.Orientation == 2 ? "landscape" : "portrait");
            }

            if ((mode.Fields & DmCopies) != 0 && mode.Copies > 1)
            {
                parts.Add($"{mode.Copies} copies");
            }

            if ((mode.Fields & DmPaperSize) != 0)
            {
                parts.Add($"paper #{mode.PaperSize}");
            }

            if ((mode.Fields & DmDefaultSource) != 0 && mode.DefaultSource != 0)
            {
                parts.Add($"tray #{mode.DefaultSource}");
            }

            if ((mode.Fields & DmDuplex) != 0 && mode.Duplex != 1)
            {
                parts.Add(mode.Duplex == 2 ? "duplex long edge" : "duplex short edge");
            }

            return parts.Count == 0 ? "saved driver settings" : string.Join(", ", parts);
        }
        catch (Exception)
        {
            // Only ever used for a log line, so a block that cannot be read is described as unknown.
            return "saved driver settings";
        }
    }

    private const int DmOrientation = 0x00000001;
    private const int DmPaperSize = 0x00000002;
    private const int DmCopies = 0x00000100;
    private const int DmDefaultSource = 0x00000400;
    private const int DmDuplex = 0x00001000;

    private static Model_DevModePublic FromBytes(byte[] bytes)
    {
        var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

        try
        {
            return Marshal.PtrToStructure<Model_DevModePublic>(handle.AddrOfPinnedObject());
        }
        finally
        {
            handle.Free();
        }
    }

    /// <summary>
    /// The public, documented part of a <c>DEVMODE</c>. The driver-defined tail is left alone: its layout
    /// belongs to the driver, and copying a block back unchanged is what keeps it valid.
    /// </summary>
    /// <remarks>
    /// The members after <c>dmFields</c> are <c>WORD</c>s in the SDK structure, not <c>DWORD</c>s. Reading
    /// them as four bytes each shifts every later field by two and reports the printer's resolution as a
    /// four-hundred-thousand copy job, which is how this was caught.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct Model_DevModePublic
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;

        public short SpecVersion;
        public short DriverVersion;
        public short Size;
        public short DriverExtra;
        public int Fields;
        public short Orientation;
        public short PaperSize;
        public short PaperLength;
        public short PaperWidth;
        public short Scale;
        public short Copies;
        public short DefaultSource;
        public short PrintQuality;
        public short Color;
        public short Duplex;
        public short YResolution;
        public short TTOption;
        public short Collate;
    }

    /// <summary>
    /// Copies bytes into a global memory block, which is the form <see cref="PrinterSettings.SetHdevmode"/>
    /// expects. A plain heap allocation would not be, and handing one over would corrupt the handle.
    /// </summary>
    private static IntPtr ToGlobalHandle(byte[] bytes)
    {
        var handle = GlobalAlloc(GmemMoveable, (UIntPtr)bytes.Length);
        if (handle == IntPtr.Zero)
        {
            return IntPtr.Zero;
        }

        var target = GlobalLock(handle);
        if (target == IntPtr.Zero)
        {
            GlobalFree(handle);
            return IntPtr.Zero;
        }

        try
        {
            Marshal.Copy(bytes, 0, target, bytes.Length);
        }
        finally
        {
            GlobalUnlock(handle);
        }

        return handle;
    }

    [DllImport("winspool.drv", EntryPoint = "OpenPrinterW", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool OpenPrinter(string printerName, out IntPtr printerHandle, IntPtr defaults);

    [DllImport("winspool.drv", EntryPoint = "ClosePrinter", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ClosePrinter(IntPtr printerHandle);

    [DllImport("winspool.drv", EntryPoint = "DocumentPropertiesW", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern int DocumentProperties(
        IntPtr ownerHandle,
        IntPtr printerHandle,
        string deviceName,
        IntPtr outputBuffer,
        IntPtr inputBuffer,
        int flags
    );

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalAlloc(uint flags, UIntPtr bytes);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalFree(IntPtr handle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalLock(IntPtr handle);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalUnlock(IntPtr handle);
}
