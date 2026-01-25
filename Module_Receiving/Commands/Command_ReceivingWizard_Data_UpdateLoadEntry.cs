using System;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models;

namespace MTM_Receiving_Application.Module_Receiving.Commands
{
    /// <summary>
    /// Update data for a single load.
    /// Clears auto-fill flags for modified fields.
    /// </summary>
    /// <param name="SessionId">The session identifier</param>
    /// <param name="LoadNumber">The load number (1 to LoadCount)</param>
    /// <param name="WeightOrQuantity">Weight or quantity value (must be > 0 if provided)</param>
    /// <param name="HeatLot">Heat lot number (max 50 chars)</param>
    /// <param name="PackageType">Package type from predefined list</param>
    /// <param name="PackagesPerLoad">Number of packages per load (must be > 0 if provided)</param>
    public record Command_ReceivingWizard_Data_UpdateLoadEntry(
        Guid SessionId,
        int LoadNumber,
        decimal? WeightOrQuantity,
        string? HeatLot,
        string? PackageType,
        int? PackagesPerLoad
    ) : IRequest<Result>;
}
