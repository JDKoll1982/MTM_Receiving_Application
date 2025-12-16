using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Data.Receiving;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Tests;

/// <summary>
/// Manual tests for Phase 1 infrastructure
/// Run these methods to verify database connectivity
/// </summary>
public static class Phase1_Manual_Tests
{
    /// <summary>
    /// T021: Test inserting a valid receiving line
    /// Expected: Success=true, AffectedRows=1
    /// </summary>
    public static async Task<bool> Test_InsertReceivingLine_ValidData()
    {
        Console.WriteLine("=== T021: Testing InsertReceivingLineAsync with valid data ===");

        var testLine = new Model_ReceivingLine
        {
            Quantity = 100,
            PartID = "TEST-PART-001",
            PONumber = 12345,
            EmployeeNumber = 1001,
            Heat = "HEAT-ABC-123",
            Date = DateTime.Today,
            InitialLocation = "A-1-1",
            CoilsOnSkid = 5,
            VendorName = "Test Vendor Corp",
            PartDescription = "Test Part Description"
        };

        try
        {
            var result = await Dao_ReceivingLine.InsertReceivingLineAsync(testLine);

            Console.WriteLine($"Success: {result.Success}");
            Console.WriteLine($"Affected Rows: {result.AffectedRows}");
            Console.WriteLine($"Execution Time: {result.ExecutionTimeMs}ms");
            Console.WriteLine($"Error Message: {result.ErrorMessage}");
            Console.WriteLine($"Severity: {result.Severity}");

            bool testPassed = result.Success && result.AffectedRows == 1;
            Console.WriteLine($"\n✓ TEST {(testPassed ? "PASSED" : "FAILED")}");

            return testPassed;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ TEST FAILED: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// T022: Test with database unavailable
    /// Expected: Success=false with descriptive error message
    /// Note: This requires manually stopping MySQL to test
    /// </summary>
    public static async Task<bool> Test_InsertReceivingLine_DatabaseUnavailable()
    {
        Console.WriteLine("=== T022: Testing InsertReceivingLineAsync with database unavailable ===");
        Console.WriteLine("NOTE: This test requires MySQL to be stopped manually");
        Console.WriteLine("Press Enter to continue or Ctrl+C to skip...");
        Console.ReadLine();

        var testLine = new Model_ReceivingLine
        {
            Quantity = 50,
            PartID = "TEST-PART-002",
            PONumber = 67890,
            EmployeeNumber = 1002,
            Heat = "HEAT-XYZ-456",
            Date = DateTime.Today,
            InitialLocation = "B-2-3"
        };

        try
        {
            var result = await Dao_ReceivingLine.InsertReceivingLineAsync(testLine);

            Console.WriteLine($"Success: {result.Success}");
            Console.WriteLine($"Error Message: {result.ErrorMessage}");
            Console.WriteLine($"Severity: {result.Severity}");

            bool testPassed = !result.Success && !string.IsNullOrEmpty(result.ErrorMessage);
            Console.WriteLine($"\n✓ TEST {(testPassed ? "PASSED" : "FAILED")}");

            return testPassed;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ TEST FAILED: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Runs all Phase 1 tests
    /// </summary>
    public static async Task RunAllTests()
    {
        Console.WriteLine("======================================");
        Console.WriteLine("PHASE 1 INFRASTRUCTURE - MANUAL TESTS");
        Console.WriteLine("======================================\n");

        int passed = 0;
        int total = 0;

        // T021
        total++;
        if (await Test_InsertReceivingLine_ValidData())
        {
            passed++;
        }
        Console.WriteLine();

        // T022 (optional - requires MySQL to be stopped)
        Console.WriteLine("Run T022 (database unavailable test)? (y/n): ");
        var response = Console.ReadLine();
        if (response?.ToLower() == "y")
        {
            total++;
            if (await Test_InsertReceivingLine_DatabaseUnavailable())
            {
                passed++;
            }
        }

        Console.WriteLine("\n======================================");
        Console.WriteLine($"RESULTS: {passed}/{total} tests passed");
        Console.WriteLine("======================================");
    }
}
