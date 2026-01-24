using System;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models;

namespace MTM_Receiving_Application.Module_Receiving.Commands
{
    /// <summary>
    /// Update Step 1 data (PO Number, Part, Load Count).
    /// Initializes LoadDetail records based on LoadCount.
    /// </summary>
    /// <param name="SessionId">The session identifier</param>
    /// <param name="PONumber">Purchase Order number (max 50 chars, alphanumeric with hyphens)</param>
    /// <param name="PartId">The part identifier from Infor Visual</param>
    /// <param name="LoadCount">Number of loads (1-100)</param>
    public record UpdateStep1Command(
        Guid SessionId,
        string PONumber,
        int PartId,
        int LoadCount
    ) : IRequest<Result>;
}
