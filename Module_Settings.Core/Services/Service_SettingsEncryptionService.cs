using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;

namespace MTM_Receiving_Application.Module_Settings.Core.Services;

/// <summary>
/// Encrypts sensitive settings values using AES with a DPAPI-protected key.
/// </summary>
public class Service_SettingsEncryptionService : ISettingsEncryptionService
{
    private const int KeySize = 32; // 256-bit
    private static readonly string KeyPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MTM_Receiving_Application",
        "Settings",
        "settings-key.bin");

    public string Encrypt(string plainText)
    {
        var key = GetOrCreateKey();
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText ?? string.Empty);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var payload = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, payload, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, payload, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(payload);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrWhiteSpace(cipherText))
        {
            return string.Empty;
        }

        try
        {
            var key = GetOrCreateKey();
            var payload = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = key;

            var ivLength = aes.BlockSize / 8;
            if (payload.Length <= ivLength)
            {
                System.Diagnostics.Trace.TraceWarning("Settings decrypt failed: payload was shorter than IV length.");
                return string.Empty;
            }

            var iv = new byte[ivLength];
            var cipherBytes = new byte[payload.Length - ivLength];

            Buffer.BlockCopy(payload, 0, iv, 0, ivLength);
            Buffer.BlockCopy(payload, ivLength, cipherBytes, 0, cipherBytes.Length);

            aes.IV = iv;
            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (FormatException ex)
        {
            System.Diagnostics.Trace.TraceError($"Settings decrypt failed: invalid base64 payload. {ex.Message}");
            return string.Empty;
        }
        catch (CryptographicException ex)
        {
            System.Diagnostics.Trace.TraceError($"Settings decrypt failed: crypto error. {ex.Message}");
            return string.Empty;
        }
        catch (ArgumentException ex)
        {
            System.Diagnostics.Trace.TraceError($"Settings decrypt failed: invalid payload. {ex.Message}");
            return string.Empty;
        }
    }

    public void RotateKey()
    {
        var directory = Path.GetDirectoryName(KeyPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var newKey = RandomNumberGenerator.GetBytes(KeySize);
        var protectedKey = ProtectedData.Protect(newKey, null, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(KeyPath, protectedKey);
    }

    private static byte[] GetOrCreateKey()
    {
        if (File.Exists(KeyPath))
        {
            try
            {
                var protectedKey = File.ReadAllBytes(KeyPath);
                return ProtectedData.Unprotect(protectedKey, null, DataProtectionScope.CurrentUser);
            }
            catch (CryptographicException ex)
            {
                System.Diagnostics.Trace.TraceError($"Settings key unprotect failed. Regenerating key. {ex.Message}");
            }
            catch (IOException ex)
            {
                System.Diagnostics.Trace.TraceError($"Settings key read failed. Regenerating key. {ex.Message}");
            }
        }

        return CreateAndPersistKey();
    }

    private static byte[] CreateAndPersistKey()
    {
        var directory = Path.GetDirectoryName(KeyPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var key = RandomNumberGenerator.GetBytes(KeySize);
        var encrypted = ProtectedData.Protect(key, null, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(KeyPath, encrypted);
        return key;
    }
}
