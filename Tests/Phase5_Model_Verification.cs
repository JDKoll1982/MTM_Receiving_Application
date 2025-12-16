using System;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Tests;

/// <summary>
/// Verification tests for Phase 5 - User Story 3 (Core Models)
/// </summary>
public static class Phase5_Model_Verification
{
    /// <summary>
    /// T035: Verify Model_ReceivingLine calculated property LabelText formats correctly
    /// </summary>
    public static bool Test_ReceivingLine_LabelText()
    {
        Console.WriteLine("=== T035: Testing Model_ReceivingLine.LabelText calculated property ===");

        var line = new Model_ReceivingLine
        {
            LabelNumber = 3,
            TotalLabels = 7
        };

        string expected = "3 / 7";
        string actual = line.LabelText;

        bool passed = actual == expected;
        Console.WriteLine($"Expected: {expected}");
        Console.WriteLine($"Actual: {actual}");
        Console.WriteLine($"✓ TEST {(passed ? "PASSED" : "FAILED")}");

        return passed;
    }

    /// <summary>
    /// T036: Verify all model properties have correct default values
    /// </summary>
    public static bool Test_Model_DefaultValues()
    {
        Console.WriteLine("\n=== T036: Testing model default values ===");

        bool allPassed = true;

        // Test Model_ReceivingLine defaults
        var receivingLine = new Model_ReceivingLine();
        Console.WriteLine("\nModel_ReceivingLine defaults:");
        Console.WriteLine($"  Date: {receivingLine.Date.Date == DateTime.Now.Date} (should be today)");
        Console.WriteLine($"  VendorName: '{receivingLine.VendorName}' == 'Unknown': {receivingLine.VendorName == "Unknown"}");
        Console.WriteLine($"  LabelNumber: {receivingLine.LabelNumber} == 1: {receivingLine.LabelNumber == 1}");

        if (receivingLine.Date.Date != DateTime.Now.Date || 
            receivingLine.VendorName != "Unknown" || 
            receivingLine.LabelNumber != 1)
        {
            allPassed = false;
        }

        // Test Model_DunnageLine defaults
        var dunnageLine = new Model_DunnageLine();
        Console.WriteLine("\nModel_DunnageLine defaults:");
        Console.WriteLine($"  Date: {dunnageLine.Date.Date == DateTime.Now.Date} (should be today)");
        Console.WriteLine($"  VendorName: '{dunnageLine.VendorName}' == 'Unknown': {dunnageLine.VendorName == "Unknown"}");
        Console.WriteLine($"  LabelNumber: {dunnageLine.LabelNumber} == 1: {dunnageLine.LabelNumber == 1}");

        if (dunnageLine.Date.Date != DateTime.Now.Date || 
            dunnageLine.VendorName != "Unknown" || 
            dunnageLine.LabelNumber != 1)
        {
            allPassed = false;
        }

        // Test Model_RoutingLabel defaults
        var routingLabel = new Model_RoutingLabel();
        Console.WriteLine("\nModel_RoutingLabel defaults:");
        Console.WriteLine($"  Date: {routingLabel.Date.Date == DateTime.Now.Date} (should be today)");
        Console.WriteLine($"  LabelNumber: {routingLabel.LabelNumber} == 1: {routingLabel.LabelNumber == 1}");

        if (routingLabel.Date.Date != DateTime.Now.Date || 
            routingLabel.LabelNumber != 1)
        {
            allPassed = false;
        }

        Console.WriteLine($"\n✓ TEST {(allPassed ? "PASSED" : "FAILED")}");
        return allPassed;
    }

    /// <summary>
    /// T037: Verify model instantiation and property types
    /// </summary>
    public static bool Test_Model_Instantiation()
    {
        Console.WriteLine("\n=== T037: Testing model instantiation and property types ===");

        bool allPassed = true;

        try
        {
            // Test Model_ReceivingLine
            var receivingLine = new Model_ReceivingLine
            {
                Id = 1,
                Quantity = 100,
                PartID = "PART-001",
                PONumber = 12345,
                EmployeeNumber = 1001,
                Heat = "HEAT-123",
                Date = DateTime.Today,
                InitialLocation = "A-1-1",
                CoilsOnSkid = 5,
                LabelNumber = 2,
                TotalLabels = 5,
                VendorName = "Test Vendor",
                PartDescription = "Test Part"
            };
            Console.WriteLine("✓ Model_ReceivingLine instantiation successful");

            // Test Model_DunnageLine
            var dunnageLine = new Model_DunnageLine
            {
                Id = 2,
                Line1 = "Line 1 Text",
                Line2 = "Line 2 Text",
                PONumber = 67890,
                Date = DateTime.Today,
                EmployeeNumber = 1002,
                VendorName = "Vendor XYZ",
                Location = "B-2-2",
                LabelNumber = 1
            };
            Console.WriteLine("✓ Model_DunnageLine instantiation successful");

            // Test Model_RoutingLabel
            var routingLabel = new Model_RoutingLabel
            {
                Id = 3,
                DeliverTo = "John Doe",
                Department = "Manufacturing",
                PackageDescription = "Metal Parts",
                PONumber = 11111,
                WorkOrderNumber = "WO-2025-001",
                EmployeeNumber = 1003,
                LabelNumber = 1,
                Date = DateTime.Today
            };
            Console.WriteLine("✓ Model_RoutingLabel instantiation successful");

            Console.WriteLine("\n✓ TEST PASSED - All models instantiate correctly");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ TEST FAILED: {ex.Message}");
            allPassed = false;
        }

        return allPassed;
    }

    /// <summary>
    /// Runs all Phase 5 verification tests
    /// </summary>
    public static void RunAllTests()
    {
        Console.WriteLine("======================================");
        Console.WriteLine("PHASE 5 - USER STORY 3: MODEL VERIFICATION");
        Console.WriteLine("======================================\n");

        int passed = 0;
        int total = 3;

        if (Test_ReceivingLine_LabelText()) passed++;
        if (Test_Model_DefaultValues()) passed++;
        if (Test_Model_Instantiation()) passed++;

        Console.WriteLine("\n======================================");
        Console.WriteLine($"RESULTS: {passed}/{total} tests passed");
        Console.WriteLine("======================================");
    }
}
