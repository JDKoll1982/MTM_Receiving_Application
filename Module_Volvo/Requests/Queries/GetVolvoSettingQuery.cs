using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Volvo.Requests.Queries;

/// <summary>
/// Query to retrieve a Volvo application setting value.
/// Used for retrieving configuration values like email recipients from the database.
/// </summary>
public record GetVolvoSettingQuery(string SettingKey) : IRequest<Model_Dao_Result<string>>
{
    /// <summary>
    /// The key of the setting to retrieve (e.g., "email_to_recipients")
    /// </summary>
    public string SettingKey { get; init; } = SettingKey;
}
