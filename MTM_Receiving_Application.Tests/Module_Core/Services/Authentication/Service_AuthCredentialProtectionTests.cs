using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Services.Authentication;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Core.Services.Authentication;

public sealed class Service_AuthCredentialProtectionTests : IDisposable
{
    private readonly string? _originalSecret;
    private readonly Service_AuthCredentialProtection _service;

    public Service_AuthCredentialProtectionTests()
    {
        _originalSecret = Environment.GetEnvironmentVariable(
            Service_AuthCredentialProtection.EnvironmentVariableName
        );

        Environment.SetEnvironmentVariable(
            Service_AuthCredentialProtection.EnvironmentVariableName,
            "unit-test-shared-secret"
        );

        _service = new Service_AuthCredentialProtection();
    }

    [Fact]
    public void HashPin_ShouldReturnSameHash_ForEquivalentTrimmedPin()
    {
        var firstHash = _service.HashPin("1234");
        var secondHash = _service.HashPin(" 1234 ");

        firstHash.Should().Be(secondHash);
        firstHash.Should().NotBe("1234");
    }

    [Fact]
    public void EncryptVisualValue_ThenDecrypt_ShouldRoundTripTrimmedValue()
    {
        var encryptedValue = _service.EncryptVisualValue(" visual-user ");
        var decryptedValue = _service.DecryptVisualValue(encryptedValue);

        encryptedValue.Should().NotBeNullOrWhiteSpace();
        encryptedValue.Should().NotBe("visual-user");
        decryptedValue.Should().Be("visual-user");
    }

    [Fact]
    public void DecryptVisualValue_ShouldReturnOriginalValue_WhenCipherTextIsInvalid()
    {
        const string invalidCipherText = "not-base64";

        var result = _service.DecryptVisualValue(invalidCipherText);

        result.Should().Be(invalidCipherText);
    }

    [Fact]
    public void HashPin_ShouldRespectFallbackSecretSources_WhenEnvironmentVariableIsMissing()
    {
        Environment.SetEnvironmentVariable(
            Service_AuthCredentialProtection.EnvironmentVariableName,
            null
        );

        Action action = () => _service.HashPin("1234");

        if (HasAvailableFallbackSecret())
        {
            action.Should().NotThrow();
            return;
        }

        action
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage($"*{Service_AuthCredentialProtection.EnvironmentVariableName}*");
    }

    private static bool HasAvailableFallbackSecret()
    {
        foreach (var secretPath in GetFallbackSecretPaths())
        {
            try
            {
                if (!File.Exists(secretPath))
                {
                    continue;
                }

                var secretValue = File.ReadAllText(secretPath);
                if (!string.IsNullOrWhiteSpace(secretValue))
                {
                    return true;
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        return false;
    }

    private static string[] GetFallbackSecretPaths()
    {
        var protectionType = typeof(Service_AuthCredentialProtection);
        var localDirectoryName = GetPrivateConstantValue(
            protectionType,
            "LocalSecretDirectoryName"
        );
        var secretFileName = GetPrivateConstantValue(protectionType, "SecretFileName");
        var sharedDirectoryPath = GetPrivateConstantValue(
            protectionType,
            "SharedSecretDirectoryPath"
        );
        var legacySharedDirectoryPath = GetPrivateConstantValue(
            protectionType,
            "LegacySharedSecretDirectoryPath"
        );

        var localSecretPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            localDirectoryName,
            secretFileName
        );
        var sharedSecretPath = Path.Combine(sharedDirectoryPath, secretFileName);
        var legacySharedSecretPath = Path.Combine(legacySharedDirectoryPath, secretFileName);

        return new[] { localSecretPath, sharedSecretPath, legacySharedSecretPath };
    }

    private static string GetPrivateConstantValue(Type declaringType, string fieldName)
    {
        var field = declaringType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);

        field.Should().NotBeNull($"the private constant '{fieldName}' should exist for the test");

        return field!.GetRawConstantValue() as string
            ?? throw new InvalidOperationException(
                $"Private constant '{fieldName}' must be a string value."
            );
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(
            Service_AuthCredentialProtection.EnvironmentVariableName,
            _originalSecret
        );
    }
}
