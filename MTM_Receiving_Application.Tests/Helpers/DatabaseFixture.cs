using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Volvo.Data;

namespace MTM_Receiving_Application.Tests.Helpers;

/// <summary>
/// Database fixture for Volvo integration tests.
/// </summary>
public class DatabaseFixture
{
    public DatabaseFixture()
    {
        ConnectionString = Helper_Database_Variables.GetConnectionString(useProduction: false);
    }

    public string ConnectionString { get; }

    public bool IsDatabaseReady { get; private set; }

    public string? DatabaseNotReadyReason { get; private set; }

    public async Task InitializeAsync()
    {
        var (isReady, reason) = await DatabasePreflight.CheckAsync(
            ConnectionString,
            requiredStoredProcedure: "sp_Volvo_PartMaster_Insert");

        IsDatabaseReady = isReady;
        DatabaseNotReadyReason = reason;
    }

    public Dao_VolvoShipment CreateShipmentDao() => new(ConnectionString);

    public Dao_VolvoShipmentLine CreateShipmentLineDao() => new(ConnectionString);

    public Dao_VolvoPart CreatePartDao() => new(ConnectionString);

    public Dao_VolvoPartComponent CreatePartComponentDao() => new(ConnectionString);
}
