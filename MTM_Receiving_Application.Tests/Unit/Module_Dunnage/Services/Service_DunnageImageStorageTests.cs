using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using FluentAssertions;
using MTM_Receiving_Application.Module_Dunnage.Services;
using Windows.Graphics.Imaging;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.Services;

public sealed class Service_DunnageImageStorageTests
{
    [Fact]
    public async Task CreateRotatedWorkingCopyAsync_ShouldSwapImageDimensions()
    {
        var service = new Service_DunnageImageStorage();
        var sourcePath = Path.Combine(
            Path.GetTempPath(),
            $"dunnage-rotate-source-{Guid.NewGuid():N}.png"
        );

        try
        {
            await WritePngAsync(sourcePath, width: 1, height: 2);

            var result = await service.CreateRotatedWorkingCopyAsync(sourcePath);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNullOrWhiteSpace();
            File.Exists(result.Data).Should().BeTrue();
            result.Data.Should().NotBe(sourcePath);

            var dimensions = await ReadDimensionsAsync(result.Data);
            dimensions.Width.Should().Be(2);
            dimensions.Height.Should().Be(1);

            File.Delete(result.Data);
        }
        finally
        {
            if (File.Exists(sourcePath))
            {
                File.Delete(sourcePath);
            }
        }
    }

    [Fact]
    public async Task CreateRotatedWorkingCopyAsync_ShouldFailWhenImageDoesNotExist()
    {
        var service = new Service_DunnageImageStorage();

        var result = await service.CreateRotatedWorkingCopyAsync(
            Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.png")
        );

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("was not found");
    }

    private static async Task WritePngAsync(string filePath, uint width, uint height)
    {
        var pixels = new byte[width * height * 4];
        Array.Fill<byte>(pixels, 255);

        await using var stream = File.Open(
            filePath,
            FileMode.Create,
            FileAccess.ReadWrite,
            FileShare.None
        );
        var encoder = await BitmapEncoder.CreateAsync(
            BitmapEncoder.PngEncoderId,
            stream.AsRandomAccessStream()
        );
        encoder.SetPixelData(
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Premultiplied,
            width,
            height,
            96,
            96,
            pixels
        );
        await encoder.FlushAsync();
    }

    private static async Task<(uint Width, uint Height)> ReadDimensionsAsync(string filePath)
    {
        await using var stream = File.Open(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read
        );
        var decoder = await BitmapDecoder.CreateAsync(stream.AsRandomAccessStream());
        return (decoder.PixelWidth, decoder.PixelHeight);
    }
}
