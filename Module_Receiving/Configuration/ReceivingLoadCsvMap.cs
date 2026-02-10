using CsvHelper.Configuration;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Configuration
{
    /// <summary>
    /// CSV class map for Model_ReceivingLoad that controls which properties are written to CSV.
    /// Only includes properties that match what's saved to the database.
    /// </summary>
    public class ReceivingLoadCsvMap : ClassMap<Model_ReceivingLoad>
    {
        public ReceivingLoadCsvMap()
        {
            // Only map properties that are saved to the database
            // Specify index to control column order
            Map(m => m.LoadID).Index(0).Name("LoadID");
            Map(m => m.PartID).Index(1).Name("PartID");
            Map(m => m.PartType).Index(2).Name("PartType");
            Map(m => m.PoNumber).Index(3).Name("PONumber");
            Map(m => m.PoLineNumber).Index(4).Name("POLineNumber");
            Map(m => m.LoadNumber).Index(5).Name("LoadNumber");
            Map(m => m.WeightQuantity).Index(6).Name("WeightQuantity");
            Map(m => m.HeatLotNumber).Index(7).Name("HeatLotNumber");
            Map(m => m.PackagesPerLoad).Index(8).Name("PackagesPerLoad");
            Map(m => m.PackageTypeName).Index(9).Name("PackageTypeName");
            Map(m => m.WeightPerPackage).Index(10).Name("WeightPerPackage");
            Map(m => m.IsNonPOItem).Index(11).Name("IsNonPOItem");
            Map(m => m.ReceivedDate).Index(12).Name("ReceivedDate");
            Map(m => m.EmployeeNumber).Index(13).Name("EmployeeNumber");
        }
    }
}


