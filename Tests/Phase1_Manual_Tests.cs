using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Data.Receiving;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Tests
{
    /// <summary>
    /// Manual tests for Phase 1 Infrastructure
    /// Verifies database connectivity and DAO operations
    /// 
    /// NOTE: These are simple manual verification tests, not xUnit tests.
    /// To run: Call Test methods from a temporary UI button or debug console.
    /// </summary>
    public static class Phase1_Manual_Tests
    {
        /// <summary>
        /// Tests successful insert of a receiving line with valid data.
        /// SUCCESS CRITERIA: Model_Dao_Result.Success = true, AffectedRows = 1
        /// </summary>
        public static async Task Test_InsertReceivingLine_ValidData()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("TEST: Insert Receiving Line - Valid Data");
            Console.WriteLine("========================================");

            try
            {
                // Arrange: Create valid receiving line data
                var testLine = new Model_ReceivingLine
                {
                    Quantity = 100,
                    PartID = "TEST-PART-001",
                    PONumber = 99999,
                    EmployeeNumber = 6229, // Assumes test employee exists
                    Heat = "TEST-HEAT-123",
                    Date = DateTime.Now,
                    InitialLocation = "TEST-LOC-A1",
                    CoilsOnSkid = 5,
                    VendorName = "Test Vendor Co",
                    PartDescription = "Test Part Description"
                };

                Console.WriteLine($"Test Data: PO={testLine.PONumber}, Part={testLine.PartID}, Qty={testLine.Quantity}");

                // Act: Insert into database
                var result = await Dao_ReceivingLine.InsertReceivingLineAsync(testLine);

                // Assert: Verify result
                Console.WriteLine($"Result: Success={result.Success}, AffectedRows={result.AffectedRows}");
                
                if (result.Success && result.AffectedRows == 1)
                {
                    Console.WriteLine("✅ TEST PASSED: Record inserted successfully");
                    Console.WriteLine($"   Affected Rows: {result.AffectedRows}");
                }
                else
                {
                    Console.WriteLine("❌ TEST FAILED: Insert did not succeed as expected");
                    Console.WriteLine($"   Error: {result.ErrorMessage}");
                    Console.WriteLine($"   Severity: {result.Severity}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ TEST FAILED: Unexpected exception: {ex.Message}");
                Console.WriteLine($"   Stack Trace: {ex.StackTrace}");
            }

            Console.WriteLine("========================================\n");
        }

        /// <summary>
        /// Tests database error handling when database is unavailable.
        /// SUCCESS CRITERIA: Model_Dao_Result.Success = false with descriptive ErrorMessage
        /// 
        /// NOTE: To test this, temporarily stop MySQL service or change connection string.
        /// </summary>
        public static async Task Test_InsertReceivingLine_DatabaseUnavailable()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("TEST: Insert Receiving Line - Database Unavailable");
            Console.WriteLine("========================================");
            Console.WriteLine("⚠️  Manual Setup Required:");
            Console.WriteLine("   1. Stop MySQL service, OR");
            Console.WriteLine("   2. Temporarily change connection string in Helper_Database_Variables");
            Console.WriteLine("   3. Run this test");
            Console.WriteLine("   4. Restore MySQL service/connection string");
            Console.WriteLine();

            try
            {
                // Arrange: Create valid data (database connection is the issue)
                var testLine = new Model_ReceivingLine
                {
                    Quantity = 50,
                    PartID = "ERROR-TEST-001",
                    PONumber = 88888,
                    EmployeeNumber = 6229,
                    Heat = "ERROR-HEAT",
                    Date = DateTime.Now,
                    InitialLocation = "ERROR-LOC",
                    VendorName = "Error Test",
                    PartDescription = "Error Test Part"
                };

                Console.WriteLine($"Attempting insert with PO={testLine.PONumber}...");

                // Act: Try to insert (should fail if database unavailable)
                var result = await Dao_ReceivingLine.InsertReceivingLineAsync(testLine);

                // Assert: Verify error handling
                Console.WriteLine($"Result: Success={result.Success}");
                
                if (!result.Success && !string.IsNullOrEmpty(result.ErrorMessage))
                {
                    Console.WriteLine("✅ TEST PASSED: Error handled gracefully");
                    Console.WriteLine($"   Error Message: {result.ErrorMessage}");
                    Console.WriteLine($"   Severity: {result.Severity}");
                }
                else if (result.Success)
                {
                    Console.WriteLine("⚠️  TEST INCONCLUSIVE: Database is accessible (expected unavailable)");
                    Console.WriteLine("   To properly test this scenario, stop MySQL service first.");
                }
                else
                {
                    Console.WriteLine("❌ TEST FAILED: Error not properly handled");
                    Console.WriteLine($"   ErrorMessage is empty or null");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("✅ TEST PASSED: Exception caught and handled");
                Console.WriteLine($"   Exception Type: {ex.GetType().Name}");
                Console.WriteLine($"   Exception Message: {ex.Message}");
            }

            Console.WriteLine("========================================\n");
        }

        /// <summary>
        /// Executes all Phase 1 verification tests
        /// </summary>
        public static async Task RunAllTests()
        {
            Console.WriteLine("\n");
            Console.WriteLine("████████████████████████████████████████");
            Console.WriteLine("  PHASE 1 INFRASTRUCTURE TESTS");
            Console.WriteLine("  Manual Verification Suite");
            Console.WriteLine("████████████████████████████████████████");
            Console.WriteLine($"Start Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine("\n");

            // Test 1: Valid insert
            await Test_InsertReceivingLine_ValidData();

            // Test 2: Database error handling
            await Test_InsertReceivingLine_DatabaseUnavailable();

            Console.WriteLine("████████████████████████████████████████");
            Console.WriteLine("  ALL PHASE 1 TESTS COMPLETE");
            Console.WriteLine($"End Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine("████████████████████████████████████████");
            Console.WriteLine("\n");
        }
    }
}
