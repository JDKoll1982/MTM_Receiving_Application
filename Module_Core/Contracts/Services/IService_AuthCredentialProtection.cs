using System;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services
{
    /// <summary>
    /// Protects authentication-related secrets before they are persisted.
    /// PINs are hashed one-way, while ERP credentials are encrypted reversibly.
    /// </summary>
    public interface IService_AuthCredentialProtection
    {
        string HashPin(string pin);

        string? EncryptVisualValue(string? plainText);

        string? DecryptVisualValue(string? cipherText);
    }
}