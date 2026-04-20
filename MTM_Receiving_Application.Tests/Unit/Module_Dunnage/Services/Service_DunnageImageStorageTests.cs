using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Helpers;
using MTM_Receiving_Application.Module_Dunnage.Services;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using Windows.Graphics.Imaging;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.Services;

public sealed class Service_DunnageImageStorageTests
{
    [Fact]
    public async Task CreateRotatedWorkingCopyAsync_ShouldSwapImageDimensions()
    {
        Helper_DunnageImagePaths.SetRootFolder(null);

        var service = new Service_DunnageImageStorage(CreateSettingsCoreFacade().Object);
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
        Helper_DunnageImagePaths.SetRootFolder(null);

        var service = new Service_DunnageImageStorage(CreateSettingsCoreFacade().Object);

        var result = await service.CreateRotatedWorkingCopyAsync(
            Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.png")
        );

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("was not found");
    }

    [Fact]
    public async Task GetConfiguredRootFolderAsync_ShouldReturnConfiguredFolder_WhenSettingExists()
    {
        var configuredFolder = Path.Combine(Path.GetTempPath(), $"dunnage-root-{Guid.NewGuid():N}");
        Directory.CreateDirectory(configuredFolder);

        try
        {
            var service = new Service_DunnageImageStorage(
                CreateSettingsCoreFacade(configuredFolder).Object
            );

            var result = await service.GetConfiguredRootFolderAsync();

            result.Should().Be(configuredFolder);
        }
        finally
        {
            if (Directory.Exists(configuredFolder))
            {
                Directory.Delete(configuredFolder, true);
            }
        }
    }

    [Fact]
    public async Task ImportTypeImageAsync_ShouldRenameFile_WhenSourceIsAlreadyUnderConfiguredRoot()
    {
        var configuredFolder = Path.Combine(Path.GetTempPath(), $"dunnage-root-{Guid.NewGuid():N}");
        var sourceFolder = Path.Combine(configuredFolder, "Incoming");
        Directory.CreateDirectory(sourceFolder);

        var sourcePath = Path.Combine(sourceFolder, "old-name.png");

        try
        {
            await WritePngAsync(sourcePath, width: 1, height: 1);

            var service = new Service_DunnageImageStorage(
                CreateSettingsCoreFacade(configuredFolder).Object
            );

            var result = await service.ImportTypeImageAsync(sourcePath, "Pallet Bin");

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be("Types/DunnageType-Pallet Bin.png");

            var targetPath = Path.Combine(configuredFolder, "Types", "DunnageType-Pallet Bin.png");
            File.Exists(targetPath).Should().BeTrue();
            File.Exists(sourcePath).Should().BeFalse();
        }
        finally
        {
            if (Directory.Exists(configuredFolder))
            {
                Directory.Delete(configuredFolder, true);
            }
        }
    }

    private static Mock<IService_SettingsCoreFacade> CreateSettingsCoreFacade(
        string? rootFolder = null
    )
    {
        var settingsCoreMock = new Mock<IService_SettingsCoreFacade>();
        settingsCoreMock
            .Setup(service =>
                service.GetSettingAsync("Dunnage", It.IsAny<string>(), It.IsAny<int?>())
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new Model_SettingsValue { Value = rootFolder ?? string.Empty }
                )
            );

        return settingsCoreMock;
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
