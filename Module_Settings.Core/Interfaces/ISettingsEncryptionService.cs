namespace MTM_Receiving_Application.Module_Settings.Core.Interfaces;

/// <summary>
/// Encrypts and decrypts sensitive settings values.
/// </summary>
public interface ISettingsEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
    void RotateKey();
}
