using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Models;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Models
{
    /// <summary>
    /// Unit tests for Model_ReceivingLoad.
    /// Tests property logic, change handlers, and calculations.
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Model")]
    public class Model_ReceivingLoad_Tests
    {
        private Model_ReceivingLoad _sut;

        public Model_ReceivingLoad_Tests()
        {
            _sut = new Model_ReceivingLoad();
        }

        // ====================================================================
        // Constructor & Defaults
        // ====================================================================

        [Fact]
        public void Constructor_Initialization_SetsDefaults()
        {
            // Act
            var model = new Model_ReceivingLoad();

            // Assert
            model.LoadID.Should().NotBeEmpty();
            model.PartID.Should().BeEmpty();
            model.HeatLotNumber.Should().BeEmpty();
            model.ReceivedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
            model.PackageType.Should().Be(Enum_PackageType.Skid);
            model.PackageTypeName.Should().Be("Skid");
            model.PackagesPerLoad.Should().Be(0);
        }

        // ====================================================================
        // Property Logic: PartID
        // ====================================================================

        [Fact]
        public void PartID_WhenChanged_SetsPackagesPerLoadToOneIfZero()
        {
            // Arrange
            _sut.PackagesPerLoad = 0;

            // Act
            _sut.PartID = "TEST-PART";

            // Assert
            _sut.PackagesPerLoad.Should().Be(1);
        }

        [Fact]
        public void PartID_WhenChangedToMMC_SetsPackageTypeToCoil()
        {
            // Arrange
            _sut.PackageType = Enum_PackageType.Skid;

            // Act
            _sut.PartID = "PART-MMC-001";

            // Assert
            _sut.PackageType.Should().Be(Enum_PackageType.Coil);
        }

        [Fact]
        public void PartID_WhenChangedToMMF_SetsPackageTypeToSheet()
        {
            // Arrange
            _sut.PackageType = Enum_PackageType.Skid;

            // Act
            _sut.PartID = "PART-MMF-001";

            // Assert
            _sut.PackageType.Should().Be(Enum_PackageType.Sheet);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void PartID_WhenSetToEmptyOrNull_DoesNotChangePackageType(string partId)
        {
            // Arrange
            _sut.PackageType = Enum_PackageType.Skid;
            _sut.PackagesPerLoad = 0;

            // Act
            _sut.PartID = partId;

            // Assert
            _sut.PackageType.Should().Be(Enum_PackageType.Skid);
            _sut.PackagesPerLoad.Should().Be(0);
        }

        // ====================================================================
        // Property Logic: PackageType / PackageTypeName
        // ====================================================================

        [Fact]
        public void PackageType_WhenChanged_UpdatesPackageTypeName()
        {
            // Act
            _sut.PackageType = Enum_PackageType.Box;

            // Assert
            _sut.PackageTypeName.Should().Be("Box");
        }

        [Fact]
        public void PackageTypeName_WhenChangedToValidEnum_UpdatesPackageType()
        {
            // Act
            _sut.PackageTypeName = "Box";

            // Assert
            _sut.PackageType.Should().Be(Enum_PackageType.Box);
        }

        [Fact]
        public void PackageTypeName_WhenChangedToInvalidEnum_DoesNotUpdatePackageType()
        {
            // Arrange
            _sut.PackageType = Enum_PackageType.Skid;

            // Act
            _sut.PackageTypeName = "InvalidType";

            // Assert
            _sut.PackageType.Should().Be(Enum_PackageType.Skid);
        }

        // ====================================================================
        // Calculation: WeightPerPackage
        // ====================================================================

        [Theory]
        [InlineData(100.0, 2, 50.0)]
        [InlineData(1000.0, 4, 250.0)]
        [InlineData(100.0, 3, 33.0)] // Rounds to nearest whole number as per logic: Math.Round(..., 0)
        [InlineData(0.0, 1, 0.0)]
        public void CalculateWeightPerPackage_UpdatesUsingWeightAndPackages(decimal weight, int packages, decimal expected)
        {
            // Act
            _sut.WeightQuantity = weight;
            _sut.PackagesPerLoad = packages;

            // Assert
            _sut.WeightPerPackage.Should().Be(expected);
        }

        [Fact]
        public void CalculateWeightPerPackage_WhenPackagesIsZero_SetResultToZero()
        {
            // Arrange
            _sut.WeightQuantity = 100;
            _sut.PackagesPerLoad = 0;

            // Act & Assert
            // Triggers OnPackagesPerLoadChanged
            _sut.WeightPerPackage.Should().Be(0);
        }

        // ====================================================================
        // Calculated Properties
        // ====================================================================

        [Fact]
        public void WeightPerPackageDisplay_FormattedCorrectly()
        {
            // Arrange
            _sut.WeightQuantity = 100;
            _sut.PackagesPerLoad = 2;
            _sut.PackageType = Enum_PackageType.Box;

            // Act
            var display = _sut.WeightPerPackageDisplay;

            // Assert
            display.Should().Be("50 lbs per Box");
        }

        [Fact]
        public void PONumberDisplay_WhenNullOrEmpty_ReturnsNA()
        {
            // Arrange
            _sut.PoNumber = null;

            // Act
            var display = _sut.PONumberDisplay;

            // Assert
            display.Should().Be("N/A");
        }

        [Fact]
        public void PONumberDisplay_WhenValid_ReturnsPONumber()
        {
            // Arrange
            _sut.PoNumber = "12345";

            // Act
            var display = _sut.PONumberDisplay;

            // Assert
            display.Should().Be("12345");
        }
    }
}
