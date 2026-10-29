using FluentAssertions;
using MTM_Receiving_Application.Module_ShipRec_Tools.Helpers;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.Helpers;

public sealed class Translation_ReceivingAnalyticsCategoryTests
{
    [Theory]
    [InlineData("Standard", "Parts")]
    [InlineData("Coil", "MMC Coils")]
    [InlineData("Sheet", "MMF Flat")]
    public void ToErpCategory_ShouldMapAppCategoryToErpCategory(string input, string expected)
    {
        Translation_ReceivingAnalyticsCategory.ToErpCategory(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("NotACategory")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ToErpCategory_ShouldReturnNull_WhenNotAnAppCategory(string? input)
    {
        Translation_ReceivingAnalyticsCategory.ToErpCategory(input).Should().BeNull();
    }

    [Theory]
    [InlineData("Parts", "Standard")]
    [InlineData("MMC Coils", "Coil")]
    [InlineData("MMF Flat", "Sheet")]
    public void ToAppCategory_ShouldMapErpCategoryToAppCategory(string input, string expected)
    {
        Translation_ReceivingAnalyticsCategory.ToAppCategory(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("Outside Service")]
    [InlineData("Uninventoried")]
    [InlineData("Unknown")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ToAppCategory_ShouldReturnNull_WhenNoAppCounterpart(string? input)
    {
        Translation_ReceivingAnalyticsCategory.ToAppCategory(input).Should().BeNull();
    }

    [Theory]
    [InlineData("MMC", "Coil")]
    [InlineData("MMF", "Sheet")]
    [InlineData("X-MMC-123", "Coil")]
    [InlineData("X-MMF-123", "Sheet")]
    [InlineData("plain part type", "Standard")]
    [InlineData("", "Standard")]
    [InlineData("   ", "Standard")]
    [InlineData(null, "Standard")]
    public void FromPartType_ShouldDeriveCategoryByContains(string? input, string expected)
    {
        Translation_ReceivingAnalyticsCategory.FromPartType(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("standard", "Parts")]
    [InlineData("COIL", "MMC Coils")]
    [InlineData("  sHeEt  ", "MMF Flat")]
    public void ToErpCategory_ShouldIgnoreCaseAndSurroundingWhitespace(
        string input,
        string expected
    )
    {
        Translation_ReceivingAnalyticsCategory.ToErpCategory(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("parts", "Standard")]
    [InlineData("mmc coils", "Coil")]
    [InlineData("  mmf flat  ", "Sheet")]
    public void ToAppCategory_ShouldIgnoreCaseAndSurroundingWhitespace(
        string input,
        string expected
    )
    {
        Translation_ReceivingAnalyticsCategory.ToAppCategory(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("mmc", "Coil")]
    [InlineData("MMF", "Sheet")]
    [InlineData("  MMC  ", "Coil")]
    public void FromPartType_ShouldIgnoreCaseAndSurroundingWhitespace(
        string input,
        string expected
    )
    {
        Translation_ReceivingAnalyticsCategory.FromPartType(input).Should().Be(expected);
    }

    [Fact]
    public void ErpCategories_ShouldHoldTheFiveNamesInContractedOrder()
    {
        Translation_ReceivingAnalyticsCategory.ErpCategories.Should().Equal(
            "Parts",
            "MMC Coils",
            "MMF Flat",
            "Outside Service",
            "Uninventoried"
        );
    }

    [Fact]
    public void AppCategories_ShouldHoldTheThreeNamesInContractedOrder()
    {
        Translation_ReceivingAnalyticsCategory.AppCategories.Should().Equal(
            "Standard",
            "Coil",
            "Sheet"
        );
    }
}
