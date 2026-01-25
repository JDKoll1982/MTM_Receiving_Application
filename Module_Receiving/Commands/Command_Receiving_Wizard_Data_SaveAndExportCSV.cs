using System;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Commands
{
    /// <summary>
    /// Save workflow data to database and CSV file.
    /// Creates transaction record and marks session as saved.
    /// </summary>
    /// <param name="SessionId">The session identifier</param>
    /// <param name="CsvOutputPath">Path where CSV file will be saved</param>
    /// <param name="SaveToDatabase">Whether to save to MySQL database (default: true)</param>
    public record Command_Receiving_Wizard_Data_SaveAndExportCSV(
        Guid SessionId,
        string CsvOutputPath,
        bool SaveToDatabase = true
    ) : IRequest<Result<Model_Receiving_Result_Save>>;
}
