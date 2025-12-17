using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using MTM_Receiving_Application.Services.Database;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.Helpers.Database;
using MTM_Receiving_Application.Contracts.Services;

namespace MTM_Receiving_Application.Tests.Integration.Database
{
    public class Service_MySQL_ReceivingTests
    {
        private readonly string _connectionString = "Server=localhost;Port=3306;Database=mtm_receiving_application;Uid=root;Pwd=root;"; // Use main DB for now as test DB might not exist
        private readonly Service_MySQL_Receiving _service;
        private readonly Mock<ILoggingService> _mockLogger;

        public Service_MySQL_ReceivingTests()
        {
            _mockLogger = new Mock<ILoggingService>();
            _service = new Service_MySQL_Receiving(_connectionString, _mockLogger.Object);
        }

        [Fact]
        public async Task SaveReceivingLoadsAsync_ShouldSaveData_WhenValid()
        {
            // Arrange
            var loads = new List<Model_ReceivingLoad>
            {
                new Model_ReceivingLoad
                {
                    LoadID = Guid.NewGuid(),
                    PartID = "TEST-PART-DB",
                    PartType = "Raw Material",
                    PoNumber = "123456",
                    PoLineNumber = "1",
                    LoadNumber = 1,
                    WeightQuantity = 500,
                    HeatLotNumber = "HEAT-DB-1",
                    PackagesPerLoad = 20,
                    PackageTypeName = "Skid",
                    WeightPerPackage = 25,
                    IsNonPOItem = false,
                    ReceivedDate = DateTime.Now
                }
            };

            // Act
            try
            {
                int savedCount = await _service.SaveReceivingLoadsAsync(loads);

                // Assert
                Assert.Equal(1, savedCount);
            }
            catch (Exception ex)
            {
                // If DB is not available, this might fail. 
                // In a real CI/CD, we'd have a dedicated test DB.
                // For local dev, we assume the developer has set up the DB via scripts.
                // If it fails, it's a valid failure indicating environment issue.
                throw; 
            }
        }
    }
}
