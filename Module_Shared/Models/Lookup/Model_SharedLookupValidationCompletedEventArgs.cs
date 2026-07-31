using System;

namespace MTM_Receiving_Application.Module_Shared.Models.Lookup;

/// <summary>
/// Event payload raised by the shared lookup textbox after validation completes.
/// </summary>
public sealed class Model_SharedLookupValidationCompletedEventArgs : EventArgs
{
    public Model_SharedLookupValidationCompletedEventArgs(
        Model_SharedLookupValidationResult result
    )
    {
        Result = result;
    }

    public Model_SharedLookupValidationResult Result { get; }
}
