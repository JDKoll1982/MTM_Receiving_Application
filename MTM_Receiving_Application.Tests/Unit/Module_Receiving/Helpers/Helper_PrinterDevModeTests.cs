using System;
using FluentAssertions;
using MTM_Receiving_Application.Module_Receiving.Helpers;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Helpers;

/// <summary>
/// Covers reading a printer's <c>DEVMODE</c> block. The fields after <c>dmFields</c> are words in the
/// SDK structure, and reading them as dwords shifted every later field — which reported a 1200 dpi
/// resolution as a two-hundred-thousand copy job and hid the tray that had been chosen.
/// </summary>
public sealed class Helper_PrinterDevModeTests
{
    private const int FieldsOffset = 72;
    private const int OrientationOffset = 76;
    private const int PaperSizeOffset = 78;
    private const int CopiesOffset = 86;
    private const int DefaultSourceOffset = 88;
    private const int DuplexOffset = 94;

    private const int DmOrientation = 0x00000001;
    private const int DmPaperSize = 0x00000002;
    private const int DmCopies = 0x00000100;
    private const int DmDefaultSource = 0x00000400;
    private const int DmDuplex = 0x00001000;

    private const short DmBinAuto = 7;
    private const short DmBinManual = 4;

    /// <summary>A block laid out the way the SDK documents it, so the reader can be checked without a printer.</summary>
    private static byte[] BuildDevMode(
        int fields,
        short orientation = 1,
        short paperSize = 1,
        short copies = 1,
        short defaultSource = 0,
        short duplex = 1
    )
    {
        var block = new byte[220];

        BitConverter.GetBytes((short)220).CopyTo(block, 68);
        BitConverter.GetBytes((short)0).CopyTo(block, 70);
        BitConverter.GetBytes(fields).CopyTo(block, FieldsOffset);

        BitConverter.GetBytes(orientation).CopyTo(block, OrientationOffset);
        BitConverter.GetBytes(paperSize).CopyTo(block, PaperSizeOffset);
        BitConverter.GetBytes(copies).CopyTo(block, CopiesOffset);
        BitConverter.GetBytes(defaultSource).CopyTo(block, DefaultSourceOffset);
        BitConverter.GetBytes(duplex).CopyTo(block, DuplexOffset);

        return block;
    }

    [Fact]
    public void Describe_ShouldReadTheTrayAndCopiesFromTheirOwnFields()
    {
        var block = BuildDevMode(
            DmOrientation | DmCopies | DmDefaultSource | DmDuplex,
            orientation: 2,
            copies: 3,
            defaultSource: DmBinAuto,
            duplex: 2
        );

        Helper_PrinterDevMode
            .Describe(block)
            .Should()
            .Be("landscape, 3 copies, tray #7, duplex long edge");
    }

    [Fact]
    public void Describe_ShouldNotInventACopyCount_WhenOneCopyIsAskedFor()
    {
        var block = BuildDevMode(DmOrientation | DmCopies, copies: 1);

        Helper_PrinterDevMode.Describe(block).Should().Be("portrait");
    }

    [Fact]
    public void Describe_ShouldReadThePaperSizeAsASingleField()
    {
        var block = BuildDevMode(DmPaperSize, paperSize: 1);

        Helper_PrinterDevMode.Describe(block).Should().Be("paper #1");
    }

    [Fact]
    public void ReadPaperSource_ShouldReturnTheTrayTheBlockNames()
    {
        var block = BuildDevMode(DmDefaultSource, defaultSource: DmBinAuto);

        Helper_PrinterDevMode.ReadPaperSource(block).Should().Be(DmBinAuto);
    }

    [Fact]
    public void ReadPaperSource_ShouldReturnNull_WhenTheBlockNamesNoTray()
    {
        // The source value is present but not flagged, so the driver is saying the tray was never set.
        var block = BuildDevMode(DmOrientation, defaultSource: DmBinManual);

        Helper_PrinterDevMode.ReadPaperSource(block).Should().BeNull();
    }

    [Fact]
    public void ReadPaperSource_ShouldReturnNull_WhenThereIsNoBlock()
    {
        Helper_PrinterDevMode.ReadPaperSource(null).Should().BeNull();
        Helper_PrinterDevMode.ReadPaperSource([]).Should().BeNull();
    }

    [Fact]
    public void Describe_ShouldSayPrinterDefaults_WhenThereIsNoBlock()
    {
        Helper_PrinterDevMode.Describe(null).Should().Be("printer defaults");
    }
}
