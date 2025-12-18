using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Moq;
using MTM_Receiving_Application.Services.Receiving;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.Contracts.Services;

namespace MTM_Receiving_Application.Tests.Unit.Services.Receiving
{
    public class Service_CSVWriterTests : IDisposable
    {
        private readonly Service_CSVWriter _service;
        private readonly string _localPath;
        private readonly Mock<IService_UserSessionManager> _mockSessionManager;

        public Service_CSVWriterTests()
        {
            _mockSessionManager = new Mock<IService_UserSessionManager>();
            _service = new Service_CSVWriter(_mockSessionManager.Object);
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _localPath = Path.Combine(appDataPath, "MTM_Receiving_Application", "ReceivingData.csv");
        }

        public void Dispose()
        {
            // Cleanup
            if (File.Exists(_localPath))
            {
                try { File.Delete(_localPath); } catch { }
            }
        }

        [Fact]
        public async Task WriteToCSVAsync_ShouldCreateFile_WhenLoadsProvided()
        {
            // Arrange
            var loads = new List<Model_ReceivingLoad>
            {
                new Model_ReceivingLoad
                {
                    PartID = "TEST-PART",
                    LoadNumber = 1,
                    WeightQuantity = 100,
                    HeatLotNumber = "HEAT-1",
                    PackagesPerLoad = 10,
                    PackageTypeName = "Box"
                }
            };

            // Act
            var result = await _service.WriteToCSVAsync(loads);

            // Assert
            Assert.True(result.LocalSuccess, $"Local write failed: {result.LocalError}");
            Assert.True(File.Exists(_localPath), $"File not found at {_localPath}");
            
            var lines = await File.ReadAllLinesAsync(_localPath);
            Assert.True(lines.Length >= 2); // Header + 1 row
            Assert.Contains("TEST-PART", lines[1]);
        }

        [Fact]
        public async Task WriteToCSVAsync_ShouldAppendToFile_WhenFileExists()
        {
            // Arrange
            var loads1 = new List<Model_ReceivingLoad>
            {
                new Model_ReceivingLoad { PartID = "PART-1", LoadNumber = 1 }
            };
            var loads2 = new List<Model_ReceivingLoad>
            {
                new Model_ReceivingLoad { PartID = "PART-2", LoadNumber = 2 }
            };

            // Act
            await _service.WriteToCSVAsync(loads1);
            await _service.WriteToCSVAsync(loads2);

            // Assert
            var lines = await File.ReadAllLinesAsync(_localPath);
            // Header + Row1 + Row2 (Note: CsvHelper behavior depends on config, assuming append is handled or file rewritten)
            // Wait, Service_CSVWriter implementation usually appends. Let's check implementation if needed.
            // Assuming standard append behavior for logs/records.
            
            // Actually, looking at typical CSV writer implementations, they might overwrite or append.
            // Let's verify if the file contains both parts.
            var content = await File.ReadAllTextAsync(_localPath);
            Assert.Contains("PART-1", content);
            Assert.Contains("PART-2", content);
        }

        [Fact]
        public async Task WriteToCSVAsync_ShouldThrowException_WhenLoadsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.WriteToCSVAsync(null));
        }
    }
}
