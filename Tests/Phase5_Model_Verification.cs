using System;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Tests
{
    /// <summary>
    /// Manual tests for Phase 5 - Model Verification
    /// Verifies model property calculations and default values
    /// 
    /// NOTE: These are simple manual verification tests, not xUnit tests.
    /// To run: Call Test methods from a temporary UI button or debug console.
    /// </summary>
    public static class Phase5_Model_Verification
    {
        /// <summary>
        /// Tests Model_ReceivingLine.LabelText property formatting.
        /// SUCCESS CRITERIA: LabelText formats as "{LabelNumber} / {TotalLabels}"
        /// </summary>
        public static void Test_ReceivingLine_LabelText()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("TEST: ReceivingLine LabelText Formatting");
            Console.WriteLine("========================================");

            try
            {
                // Test Case 1: Standard label numbering
                var line1 = new Model_ReceivingLine
                {
                    LabelNumber = 3,
                    TotalLabels = 5
                };

                string expected1 = "3 / 5";
                string actual1 = line1.LabelText;

                Console.WriteLine($"Test Case 1: LabelNumber={line1.LabelNumber}, TotalLabels={line1.TotalLabels}");
                Console.WriteLine($"   Expected: '{expected1}'");
                Console.WriteLine($"   Actual:   '{actual1}'");
                
                if (actual1 == expected1)
                {
                    Console.WriteLine("   ✅ PASSED");
                }
                else
                {
                    Console.WriteLine("   ❌ FAILED");
                }

                // Test Case 2: Single label
                var line2 = new Model_ReceivingLine
                {
                    LabelNumber = 1,
                    TotalLabels = 1
                };

                string expected2 = "1 / 1";
                string actual2 = line2.LabelText;

                Console.WriteLine($"\nTest Case 2: LabelNumber={line2.LabelNumber}, TotalLabels={line2.TotalLabels}");
                Console.WriteLine($"   Expected: '{expected2}'");
                Console.WriteLine($"   Actual:   '{actual2}'");
                
                if (actual2 == expected2)
                {
                    Console.WriteLine("   ✅ PASSED");
                }
                else
                {
                    Console.WriteLine("   ❌ FAILED");
                }

                // Test Case 3: Multiple labels (last label)
                var line3 = new Model_ReceivingLine
                {
                    LabelNumber = 10,
                    TotalLabels = 10
                };

                string expected3 = "10 / 10";
                string actual3 = line3.LabelText;

                Console.WriteLine($"\nTest Case 3: LabelNumber={line3.LabelNumber}, TotalLabels={line3.TotalLabels}");
                Console.WriteLine($"   Expected: '{expected3}'");
                Console.WriteLine($"   Actual:   '{actual3}'");
                
                if (actual3 == expected3)
                {
                    Console.WriteLine("   ✅ PASSED");
                }
                else
                {
                    Console.WriteLine("   ❌ FAILED");
                }

                Console.WriteLine("\n✅ TEST COMPLETE: LabelText formatting verification");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ TEST FAILED: Unexpected exception: {ex.Message}");
            }

            Console.WriteLine("========================================\n");
        }

        /// <summary>
        /// Tests default values for model properties.
        /// SUCCESS CRITERIA: All models have appropriate default values
        /// </summary>
        public static void Test_Model_DefaultValues()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("TEST: Model Default Values");
            Console.WriteLine("========================================");

            try
            {
                // Test Model_ReceivingLine defaults
                var receivingLine = new Model_ReceivingLine();
                bool allDefaultsCorrect = true;

                Console.WriteLine("Model_ReceivingLine Defaults:");
                
                // Date should default to DateTime.Now (within 1 minute tolerance)
                var dateDiff = Math.Abs((receivingLine.Date - DateTime.Now).TotalSeconds);
                if (dateDiff < 60)
                {
                    Console.WriteLine($"   ✅ Date: {receivingLine.Date:yyyy-MM-dd HH:mm} (defaults to DateTime.Now)");
                }
                else
                {
                    Console.WriteLine($"   ❌ Date: {receivingLine.Date:yyyy-MM-dd HH:mm} (not within 1 minute of Now)");
                    allDefaultsCorrect = false;
                }

                // VendorName should default to "Unknown"
                if (receivingLine.VendorName == "Unknown")
                {
                    Console.WriteLine($"   ✅ VendorName: '{receivingLine.VendorName}' (defaults to 'Unknown')");
                }
                else
                {
                    Console.WriteLine($"   ❌ VendorName: '{receivingLine.VendorName}' (expected 'Unknown')");
                    allDefaultsCorrect = false;
                }

                // LabelNumber should default to 1
                if (receivingLine.LabelNumber == 1)
                {
                    Console.WriteLine($"   ✅ LabelNumber: {receivingLine.LabelNumber} (defaults to 1)");
                }
                else
                {
                    Console.WriteLine($"   ❌ LabelNumber: {receivingLine.LabelNumber} (expected 1)");
                    allDefaultsCorrect = false;
                }

                // TotalLabels should default to 1
                if (receivingLine.TotalLabels == 1)
                {
                    Console.WriteLine($"   ✅ TotalLabels: {receivingLine.TotalLabels} (defaults to 1)");
                }
                else
                {
                    Console.WriteLine($"   ❌ TotalLabels: {receivingLine.TotalLabels} (expected 1)");
                    allDefaultsCorrect = false;
                }

                // Test Model_DunnageLine defaults
                Console.WriteLine("\nModel_DunnageLine Defaults:");
                var dunnageLine = new Model_DunnageLine();
                
                // Date should default to DateTime.Now
                dateDiff = Math.Abs((dunnageLine.Date - DateTime.Now).TotalSeconds);
                if (dateDiff < 60)
                {
                    Console.WriteLine($"   ✅ Date: {dunnageLine.Date:yyyy-MM-dd HH:mm} (defaults to DateTime.Now)");
                }
                else
                {
                    Console.WriteLine($"   ❌ Date: {dunnageLine.Date:yyyy-MM-dd HH:mm} (not within 1 minute of Now)");
                    allDefaultsCorrect = false;
                }

                // VendorName should default to "Unknown"
                if (dunnageLine.VendorName == "Unknown")
                {
                    Console.WriteLine($"   ✅ VendorName: '{dunnageLine.VendorName}' (defaults to 'Unknown')");
                }
                else
                {
                    Console.WriteLine($"   ❌ VendorName: '{dunnageLine.VendorName}' (expected 'Unknown')");
                    allDefaultsCorrect = false;
                }

                // LabelNumber should default to 1
                if (dunnageLine.LabelNumber == 1)
                {
                    Console.WriteLine($"   ✅ LabelNumber: {dunnageLine.LabelNumber} (defaults to 1)");
                }
                else
                {
                    Console.WriteLine($"   ❌ LabelNumber: {dunnageLine.LabelNumber} (expected 1)");
                    allDefaultsCorrect = false;
                }

                // Test Model_CarrierDeliveryLabel defaults
                Console.WriteLine("\nModel_CarrierDeliveryLabel Defaults:");
                var carrierDeliveryLabel = new Model_CarrierDeliveryLabel();
                
                // Date should default to DateTime.Now
                dateDiff = Math.Abs((carrierDeliveryLabel.Date - DateTime.Now).TotalSeconds);
                if (dateDiff < 60)
                {
                    Console.WriteLine($"   ✅ Date: {carrierDeliveryLabel.Date:yyyy-MM-dd HH:mm} (defaults to DateTime.Now)");
                }
                else
                {
                    Console.WriteLine($"   ❌ Date: {carrierDeliveryLabel.Date:yyyy-MM-dd HH:mm} (not within 1 minute of Now)");
                    allDefaultsCorrect = false;
                }

                // LabelNumber should default to 1
                if (carrierDeliveryLabel.LabelNumber == 1)
                {
                    Console.WriteLine($"   ✅ LabelNumber: {carrierDeliveryLabel.LabelNumber} (defaults to 1)");
                }
                else
                {
                    Console.WriteLine($"   ❌ LabelNumber: {carrierDeliveryLabel.LabelNumber} (expected 1)");
                    allDefaultsCorrect = false;
                }

                Console.WriteLine();
                if (allDefaultsCorrect)
                {
                    Console.WriteLine("✅ TEST COMPLETE: All default values are correct");
                }
                else
                {
                    Console.WriteLine("❌ TEST FAILED: Some default values are incorrect");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ TEST FAILED: Unexpected exception: {ex.Message}");
            }

            Console.WriteLine("========================================\n");
        }

        /// <summary>
        /// Executes all model verification tests
        /// </summary>
        public static void RunAllTests()
        {
            Console.WriteLine("\n");
            Console.WriteLine("████████████████████████████████████████");
            Console.WriteLine("  PHASE 5 MODEL VERIFICATION TESTS");
            Console.WriteLine("  Property Validation Suite");
            Console.WriteLine("████████████████████████████████████████");
            Console.WriteLine($"Start Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine("\n");

            // Test 1: LabelText formatting
            Test_ReceivingLine_LabelText();

            // Test 2: Default values
            Test_Model_DefaultValues();

            Console.WriteLine("████████████████████████████████████████");
            Console.WriteLine("  ALL MODEL VERIFICATION TESTS COMPLETE");
            Console.WriteLine($"End Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine("████████████████████████████████████████");
            Console.WriteLine("\n");
        }
    }
}
