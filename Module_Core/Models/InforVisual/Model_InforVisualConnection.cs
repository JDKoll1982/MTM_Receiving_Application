namespace MTM_Receiving_Application.Module_Core.Models.InforVisual;

using MTM_Receiving_Application.Module_Core.Defaults;

/// <summary>
/// Configuration for Infor Visual database connection
/// READ-ONLY access to VISUAL server, MTMFG database
/// </summary>
public class Model_InforVisualConnection
{
    public string Server { get; set; } = InforVisualDefaults.DefaultServer;
    public string Database { get; set; } = InforVisualDefaults.DefaultDatabase;
    public string UserId { get; set; } = "SHOP2";
    public string Password { get; set; } = "SHOP";
    public string SiteId { get; set; } = InforVisualDefaults.DefaultSiteId;

    /// <summary>
    /// Builds READ-ONLY connection string for Infor Visual
    /// </summary>
    public string GetConnectionString()
    {
        return $"Server={Server};Database={Database};User Id={UserId};Password={Password};TrustServerCertificate=True;ApplicationIntent=ReadOnly;";
    }
}

