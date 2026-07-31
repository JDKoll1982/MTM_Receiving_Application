namespace MTM_Receiving_Application.Module_Scanner.Models;

public sealed class Model_ScannerLocationValidationResult
{
    public bool IsValid { get; init; }

    public string Message { get; init; } = string.Empty;

    public string CanonicalLocation { get; init; } = string.Empty;
}