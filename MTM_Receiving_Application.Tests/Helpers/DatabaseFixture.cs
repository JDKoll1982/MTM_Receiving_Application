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
        ConnectionString = Helper_Database_Variables.GetConnectionString();
    }

    public string ConnectionString { get; }

    public Dao_VolvoShipment CreateShipmentDao() => new(ConnectionString);

    public Dao_VolvoShipmentLine CreateShipmentLineDao() => new(ConnectionString);

    public Dao_VolvoPart CreatePartDao() => new(ConnectionString);

    public Dao_VolvoPartComponent CreatePartComponentDao() => new(ConnectionString);
}
