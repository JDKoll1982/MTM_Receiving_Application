using System;
using System.IO;
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
        private const string LocalSecretDirectoryName = "MTM Receiving Application\\Security";
        private const string SecretFileName = "MTM_AUTH_USER_SECRET_KEY.txt";
        private const string SharedSecretDirectoryPath =
            "\\\\172.16.1.104\\MTM_Receiving_Application\\Security";

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
            if (TryNormalizeSecret(masterSecret, out var normalizedMasterSecret))
            {
                return normalizedMasterSecret;
            }

            if (TryLoadSecretFromFallbackPaths(out normalizedMasterSecret))
            {
                CacheSecretForCurrentWorkstation(normalizedMasterSecret);
                return normalizedMasterSecret;
            }

            if (string.IsNullOrWhiteSpace(masterSecret))
            {
                throw new InvalidOperationException(
                    $"Environment variable '{EnvironmentVariableName}' is missing and no local or shared auth secret file could be found for this workstation."
                );
            }

            return masterSecret.Trim();
        }

        private static bool TryLoadSecretFromFallbackPaths(out string masterSecret)
        {
            foreach (var secretPath in GetFallbackSecretPaths())
            {
                try
                {
                    if (!File.Exists(secretPath))
                    {
                        continue;
                    }

                    var fileSecret = File.ReadAllText(secretPath, Encoding.UTF8);
                    if (TryNormalizeSecret(fileSecret, out masterSecret))
                    {
                        return true;
                    }
                }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }

            masterSecret = string.Empty;
            return false;
        }

        private static string[] GetFallbackSecretPaths()
        {
            var programDataPath = Environment.GetFolderPath(
                Environment.SpecialFolder.CommonApplicationData
            );
            var localSecretPath = Path.Combine(
                programDataPath,
                LocalSecretDirectoryName,
                SecretFileName
            );
            var sharedSecretPath = Path.Combine(SharedSecretDirectoryPath, SecretFileName);

            return new[] { localSecretPath, sharedSecretPath };
        }

        private static void CacheSecretForCurrentWorkstation(string masterSecret)
        {
            Environment.SetEnvironmentVariable(
                EnvironmentVariableName,
                masterSecret,
                EnvironmentVariableTarget.Process
            );

            try
            {
                Environment.SetEnvironmentVariable(
                    EnvironmentVariableName,
                    masterSecret,
                    EnvironmentVariableTarget.Machine
                );
            }
            catch (UnauthorizedAccessException)
            {
                try
                {
                    Environment.SetEnvironmentVariable(
                        EnvironmentVariableName,
                        masterSecret,
                        EnvironmentVariableTarget.User
                    );
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (System.Security.SecurityException)
                {
                }
            }
            catch (System.Security.SecurityException)
            {
                try
                {
                    Environment.SetEnvironmentVariable(
                        EnvironmentVariableName,
                        masterSecret,
                        EnvironmentVariableTarget.User
                    );
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (System.Security.SecurityException)
                {
                }
            }

            try
            {
                var localSecretDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    LocalSecretDirectoryName
                );

                Directory.CreateDirectory(localSecretDirectory);
                var localSecretPath = Path.Combine(localSecretDirectory, SecretFileName);
                File.WriteAllText(localSecretPath, masterSecret, new UTF8Encoding(false));
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        private static bool TryNormalizeSecret(string? candidateSecret, out string normalizedSecret)
        {
            if (string.IsNullOrWhiteSpace(candidateSecret))
            {
                normalizedSecret = string.Empty;
                return false;
            }

            normalizedSecret = candidateSecret.Trim();
            return normalizedSecret.Length > 0;
        }
    }
}
