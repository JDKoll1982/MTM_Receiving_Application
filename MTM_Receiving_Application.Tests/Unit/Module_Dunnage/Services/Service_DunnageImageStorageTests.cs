using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Helpers;
using MTM_Receiving_Application.Module_Dunnage.Services;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using Windows.Graphics.Imaging;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage.Services;

public sealed class Service_DunnageImageStorageTests
{
    [Fact]
    public async Task CreateRotatedWorkingCopyAsync_ShouldSwapImageDimensions()
    {
        var localCacheRoot = Path.Combine(Path.GetTempPath(), $"dunnage-cache-{Guid.NewGuid():N}");
        Helper_DunnageImagePaths.SetRootFolder(null);
        Helper_DunnageImagePaths.SetLocalCacheRootFolder(localCacheRoot);

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

            if (Directory.Exists(localCacheRoot))
            {
                Directory.Delete(localCacheRoot, true);
            }
        }
    }

    [Fact]
    public async Task CreateRotatedWorkingCopyAsync_ShouldFailWhenImageDoesNotExist()
    {
        var localCacheRoot = Path.Combine(Path.GetTempPath(), $"dunnage-cache-{Guid.NewGuid():N}");
        Helper_DunnageImagePaths.SetRootFolder(null);
        Helper_DunnageImagePaths.SetLocalCacheRootFolder(localCacheRoot);

        var service = new Service_DunnageImageStorage(CreateSettingsCoreFacade().Object);

        var result = await service.CreateRotatedWorkingCopyAsync(
            Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.png")
        );

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("was not found");

        if (Directory.Exists(localCacheRoot))
        {
            Directory.Delete(localCacheRoot, true);
        }
    }

    [Fact]
    public async Task GetConfiguredRootFolderAsync_ShouldReturnConfiguredFolder_WhenSettingExists()
    {
        var configuredFolder = Path.Combine(Path.GetTempPath(), $"dunnage-root-{Guid.NewGuid():N}");
        var localCacheRoot = Path.Combine(Path.GetTempPath(), $"dunnage-cache-{Guid.NewGuid():N}");
        Directory.CreateDirectory(configuredFolder);
        Helper_DunnageImagePaths.SetLocalCacheRootFolder(localCacheRoot);

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

            if (Directory.Exists(localCacheRoot))
            {
                Directory.Delete(localCacheRoot, true);
            }
        }
    }

    [Fact]
    public async Task ImportTypeImageAsync_ShouldRenameFile_WhenSourceIsAlreadyUnderConfiguredRoot()
    {
        var configuredFolder = Path.Combine(Path.GetTempPath(), $"dunnage-root-{Guid.NewGuid():N}");
        var localCacheRoot = Path.Combine(Path.GetTempPath(), $"dunnage-cache-{Guid.NewGuid():N}");
        var sourceFolder = Path.Combine(configuredFolder, "Incoming");
        Directory.CreateDirectory(sourceFolder);
        Helper_DunnageImagePaths.SetLocalCacheRootFolder(localCacheRoot);

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

            if (Directory.Exists(localCacheRoot))
            {
                Directory.Delete(localCacheRoot, true);
            }
        }
    }

    [Fact]
    public async Task ImportPartImageAsync_ShouldRenameFile_WhenSourceIsAlreadyUnderConfiguredRoot()
    {
        var configuredFolder = Path.Combine(Path.GetTempPath(), $"dunnage-root-{Guid.NewGuid():N}");
        var localCacheRoot = Path.Combine(Path.GetTempPath(), $"dunnage-cache-{Guid.NewGuid():N}");
        var sourceFolder = Path.Combine(configuredFolder, "Incoming");
        Directory.CreateDirectory(sourceFolder);
        Helper_DunnageImagePaths.SetLocalCacheRootFolder(localCacheRoot);

        var sourcePath = Path.Combine(sourceFolder, "old-name.jpg");

        try
        {
            await WritePngAsync(sourcePath, width: 1, height: 1);

            var service = new Service_DunnageImageStorage(
                CreateSettingsCoreFacade(configuredFolder).Object
            );

            var result = await service.ImportPartImageAsync(sourcePath, "Pallet Bin", "PART-300");

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be("Parts/Pallet Bin-PART-300.jpg");

            var targetPath = Path.Combine(configuredFolder, "Parts", "Pallet Bin-PART-300.jpg");
            File.Exists(targetPath).Should().BeTrue();
            File.Exists(sourcePath).Should().BeFalse();
        }
        finally
        {
            if (Directory.Exists(configuredFolder))
            {
                Directory.Delete(configuredFolder, true);
            }

            if (Directory.Exists(localCacheRoot))
            {
                Directory.Delete(localCacheRoot, true);
            }
        }
    }

    [Fact]
    public async Task SyncLocalCacheAsync_ShouldMirrorConfiguredFolderIntoLocalCache()
    {
        var configuredFolder = Path.Combine(Path.GetTempPath(), $"dunnage-root-{Guid.NewGuid():N}");
        var localCacheRoot = Path.Combine(Path.GetTempPath(), $"dunnage-cache-{Guid.NewGuid():N}");
        var partsFolder = Path.Combine(configuredFolder, "Parts");
        Directory.CreateDirectory(partsFolder);
        Directory.CreateDirectory(localCacheRoot);
        Helper_DunnageImagePaths.SetLocalCacheRootFolder(localCacheRoot);

        var sharedImagePath = Path.Combine(partsFolder, "Pallet-PART-300.png");
        var staleLocalImagePath = Path.Combine(localCacheRoot, "Parts", "obsolete.png");

        try
        {
            await WritePngAsync(sharedImagePath, width: 2, height: 2);
            Directory.CreateDirectory(Path.GetDirectoryName(staleLocalImagePath)!);
            await WritePngAsync(staleLocalImagePath, width: 1, height: 1);

            var service = new Service_DunnageImageStorage(
                CreateSettingsCoreFacade(configuredFolder).Object
            );

            await service.SyncLocalCacheAsync();

            var cachedImagePath = Path.Combine(localCacheRoot, "Parts", "Pallet-PART-300.png");
            File.Exists(cachedImagePath).Should().BeTrue();
            File.Exists(staleLocalImagePath).Should().BeFalse();
        }
        finally
        {
            if (Directory.Exists(configuredFolder))
            {
                Directory.Delete(configuredFolder, true);
            }

            if (Directory.Exists(localCacheRoot))
            {
                Directory.Delete(localCacheRoot, true);
            }
        }
    }

    [Fact]
    public void GetDisplayAbsolutePath_ShouldPreferLocalCacheAndFallbackToConfiguredRoot()
    {
        var configuredFolder = Path.Combine(Path.GetTempPath(), $"dunnage-root-{Guid.NewGuid():N}");
        var localCacheRoot = Path.Combine(Path.GetTempPath(), $"dunnage-cache-{Guid.NewGuid():N}");
        var relativePath = Path.Combine("Parts", "Pallet-PART-300.png");
        var sharedImagePath = Path.Combine(configuredFolder, relativePath);
        var localImagePath = Path.Combine(localCacheRoot, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(sharedImagePath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(localImagePath)!);
        Helper_DunnageImagePaths.SetRootFolder(configuredFolder);
        Helper_DunnageImagePaths.SetLocalCacheRootFolder(localCacheRoot);

        try
        {
            File.WriteAllText(sharedImagePath, "shared");
            Helper_DunnageImagePaths
                .GetDisplayAbsolutePath(relativePath)
                .Should()
                .Be(sharedImagePath);

            File.WriteAllText(localImagePath, "local");
            Helper_DunnageImagePaths
                .GetDisplayAbsolutePath(relativePath)
                .Should()
                .Be(localImagePath);
        }
        finally
        {
            if (Directory.Exists(configuredFolder))
            {
                Directory.Delete(configuredFolder, true);
            }

            if (Directory.Exists(localCacheRoot))
            {
                Directory.Delete(localCacheRoot, true);
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
