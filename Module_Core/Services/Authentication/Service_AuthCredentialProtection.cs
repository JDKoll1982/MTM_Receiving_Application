using System;
using System.Security.Cryptography;
using System.Text;
using MTM_Receiving_Application.Module_Core.Contracts.Services;

namespace MTM_Receiving_Application.Module_Core.Services.Authentication
{
    /// <summary>
    /// Protects auth user secrets using a shared environment-variable key.
    /// </summary>
    public class Service_AuthCredentialProtection : IService_AuthCredentialProtection
    {
        public const string EnvironmentVariableName = "MTM_AUTH_USER_SECRET_KEY";

        private const int DerivedKeySize = 32;
        private const int PinIterationCount = 100000;

        public string HashPin(string pin)
        {
            if (string.IsNullOrWhiteSpace(pin))
            {
                throw new ArgumentException("PIN is required.", nameof(pin));
            }

            var normalizedPin = pin.Trim();
            var salt = DerivePurposeBytes("PIN");
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                normalizedPin,
                salt,
                PinIterationCount,
                HashAlgorithmName.SHA256,
                DerivedKeySize
            );

            return Convert.ToBase64String(hash);
        }

        public string? EncryptVisualValue(string? plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText))
            {
                return null;
            }

            using var aes = Aes.Create();
            aes.Key = DerivePurposeBytes("ERP");
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText.Trim());
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            var payload = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, payload, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, payload, aes.IV.Length, cipherBytes.Length);

            return Convert.ToBase64String(payload);
        }

        public string? DecryptVisualValue(string? cipherText)
        {
            if (string.IsNullOrWhiteSpace(cipherText))
            {
                return null;
            }

            try
            {
                var payload = Convert.FromBase64String(cipherText);

                using var aes = Aes.Create();
                aes.Key = DerivePurposeBytes("ERP");

                var ivLength = aes.BlockSize / 8;
                if (payload.Length <= ivLength)
                {
                    return cipherText;
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
            catch (FormatException)
            {
                return cipherText;
            }
            catch (CryptographicException)
            {
                return cipherText;
            }
            catch (ArgumentException)
            {
                return cipherText;
            }
        }

        private static byte[] DerivePurposeBytes(string purpose)
        {
            var masterSecret = GetMasterSecret();
            return SHA256.HashData(Encoding.UTF8.GetBytes($"{purpose}|{masterSecret}"));
        }

        private static string GetMasterSecret()
        {
            var masterSecret = Environment.GetEnvironmentVariable(EnvironmentVariableName);
            if (string.IsNullOrWhiteSpace(masterSecret))
            {
                throw new InvalidOperationException(
                    $"Environment variable '{EnvironmentVariableName}' must be configured on each client workstation before authentication data can be read or written."
                );
            }

            return masterSecret.Trim();
        }
    }
}