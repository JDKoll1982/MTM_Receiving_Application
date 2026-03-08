namespace Visual_Inventory_Assistant.Classes;

public class ApplicationVariables
{
    #region Properties

    public static string? VisualUserName { get; set; } = string.Empty;

    public static string? VisualPassword { get; set; } = string.Empty;

    public static string VisualServer { get; set; } = "MTMFG";

    public static string? ApplicationUserName { get; set; } = string.Empty;

    public static string? ApplicationPassword { get; set; } = string.Empty;

    public static string Server { get; set; } = string.Empty;

    public static string GoogleSheetsLink { get; set; } =
        "https://docs.google.com/spreadsheets/d/1QO0byGw_hJ35FpQUnUaZw_16zhiHXwVgrppB4-JF2TY/edit?usp=sharing";

    public static string GoogleSheetsSheet { get; set; } = "Tally Sheet";

    public static string DefaultSqlServer { get; set; } = string.Empty;

    public static string DefaultSqlUserName { get; set; } = string.Empty;

    public static string DefaultSqlPassword { get; set; } = string.Empty;

    public static string CurrentPartId { get; set; } = string.Empty;

    public static string CurrentQuantity { get; set; } = string.Empty;

    public static string CurrentFromLocation { get; set; } = string.Empty;

    public static string CurrentToLocation { get; set; } = string.Empty;

    public static int CurrentTransaction { get; set; } = int.MinValue;

    #endregion
}