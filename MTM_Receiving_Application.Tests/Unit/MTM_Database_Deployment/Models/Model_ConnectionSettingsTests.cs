using FluentAssertions;
using MTM_Database_Deployment.Models;

namespace MTM_Receiving_Application.Tests.Unit.MTM_Database_Deployment.Models;

/// <summary>
/// The database picker must never silently retarget a deployment. These tests pin the list the
/// operator sees and the wording that tells them which one holds real data.
/// </summary>
public sealed class Model_ConnectionSettingsTests
{
    [Fact]
    public void ResolveAvailableDatabases_ShouldOfferLiveAndTest_WhenConfiguredWithTheDefaults()
    {
        var settings = new Model_ConnectionSettings();

        settings.ResolveAvailableDatabases().Should().Equal(
            Model_ConnectionSettings.LiveDatabase,
            Model_ConnectionSettings.TestDatabase);
    }

    [Fact]
    public void ResolveAvailableDatabases_ShouldKeepTheSelectedDatabase_WhenItIsNotInTheList()
    {
        var settings = new Model_ConnectionSettings
        {
            Database = "mtm_custom",
            AvailableDatabases = [Model_ConnectionSettings.LiveDatabase, Model_ConnectionSettings.TestDatabase]
        };

        var databases = settings.ResolveAvailableDatabases();

        databases.Should().StartWith("mtm_custom");
        databases.Should().HaveCount(3);
    }

    [Fact]
    public void ResolveAvailableDatabases_ShouldNotDuplicate_WhenTheListAlreadyContainsTheSelection()
    {
        var settings = new Model_ConnectionSettings
        {
            Database = Model_ConnectionSettings.TestDatabase,
            AvailableDatabases = [Model_ConnectionSettings.LiveDatabase, Model_ConnectionSettings.TestDatabase]
        };

        settings.ResolveAvailableDatabases().Should().HaveCount(2);
    }

    [Fact]
    public void ResolveAvailableDatabases_ShouldIgnoreBlanksAndCaseDuplicates()
    {
        var settings = new Model_ConnectionSettings
        {
            Database = string.Empty,
            AvailableDatabases = ["  ", Model_ConnectionSettings.LiveDatabase, "MTM_RECEIVING_APPLICATION", null!]
        };

        settings.ResolveAvailableDatabases().Should().ContainSingle()
            .Which.Should().Be(Model_ConnectionSettings.LiveDatabase);
    }

    [Fact]
    public void ResolveAvailableDatabases_ShouldFallBackToLive_WhenNothingIsConfigured()
    {
        var settings = new Model_ConnectionSettings
        {
            Database = string.Empty,
            AvailableDatabases = []
        };

        settings.ResolveAvailableDatabases().Should().Equal(Model_ConnectionSettings.LiveDatabase);
    }

    [Fact]
    public void Clone_ShouldCopyTheDatabaseList_WhenTheSourceIsMutatedLater()
    {
        var settings = new Model_ConnectionSettings();
        var clone = settings.Clone();

        settings.AvailableDatabases.Add("mtm_added_later");

        clone.AvailableDatabases.Should().HaveCount(2);
    }

    [Fact]
    public void For_ShouldWarnThatTheLiveDatabaseIsReal_WhenNameHasNoTestSuffix()
    {
        var option = Model_DatabaseOption.For(Model_ConnectionSettings.LiveDatabase);

        option.Title.Should().Be("Live database");
        option.Description.Should().Contain("real");
        option.Display.Should().Be("Live database - mtm_receiving_application");
    }

    [Fact]
    public void For_ShouldCallItAPracticeCopy_WhenNameEndsWithTest()
    {
        var option = Model_DatabaseOption.For("mtm_receiving_application_test");

        option.Title.Should().Be("Test database");
        option.Description.Should().Contain("practice copy");
    }

    [Fact]
    public void ToString_ShouldMatchDisplay_SoThePickerIsNotAnnouncedByTypeName()
    {
        var option = Model_DatabaseOption.For(Model_ConnectionSettings.LiveDatabase);

        option.ToString().Should().Be(option.Display);
    }
}
