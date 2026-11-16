using FluentAssertions;
using MTM_Database_Deployment.Models;
using MTM_Database_Deployment.Services;

namespace MTM_Receiving_Application.Tests.Unit.MTM_Database_Deployment.Services;

/// <summary>
/// A safe update must not write seed data a second time. The ledger is what remembers that a seed
/// already landed in a database, so its scoping and fingerprint rules decide whether reference data
/// is applied once or on every run.
/// </summary>
public sealed class Service_SeedLedgerTests
{
    [Fact]
    public void WasApplied_ShouldBeFalse_WhenTheLedgerHasNeverSeenTheSeed()
    {
        var ledger = new Service_SeedLedger(new Model_ConnectionSettings());
        var scope = Service_SeedLedger.ScopeFor("172.16.1.104", 3306, "mtm_receiving_application");

        ledger.WasApplied(scope, "SeedData/01_seed_volvo_data.sql", Service_SeedLedger.Fingerprint("INSERT ..."))
            .Should()
            .BeFalse();
    }

    [Fact]
    public void WasApplied_ShouldBeTrue_WhenTheSeedWasRecordedWithTheSameText()
    {
        var ledger = new Service_SeedLedger(new Model_ConnectionSettings());
        var scope = Service_SeedLedger.ScopeFor("172.16.1.104", 3306, "mtm_receiving_application");
        var fingerprint = Service_SeedLedger.Fingerprint("INSERT INTO t VALUES (1);");

        ledger.Record(scope, "SeedData/01_seed_volvo_data.sql", fingerprint);

        ledger.WasApplied(scope, "SeedData/01_seed_volvo_data.sql", fingerprint).Should().BeTrue();
    }

    [Fact]
    public void WasApplied_ShouldBeFalse_WhenTheSeedFileWasEditedSinceItRan()
    {
        var ledger = new Service_SeedLedger(new Model_ConnectionSettings());
        var scope = Service_SeedLedger.ScopeFor("172.16.1.104", 3306, "mtm_receiving_application");

        ledger.Record(
            scope,
            "SeedData/01_seed_volvo_data.sql",
            Service_SeedLedger.Fingerprint("INSERT INTO t VALUES (1);"));

        ledger.WasApplied(
                scope,
                "SeedData/01_seed_volvo_data.sql",
                Service_SeedLedger.Fingerprint("INSERT INTO t VALUES (1); INSERT INTO t VALUES (2);"))
            .Should()
            .BeFalse("an edited seed still has to reach the database");
    }

    [Fact]
    public void WasApplied_ShouldBeFalse_WhenTheSameSeedRanAgainstAnotherDatabase()
    {
        var ledger = new Service_SeedLedger(new Model_ConnectionSettings());
        var live = Service_SeedLedger.ScopeFor("172.16.1.104", 3306, "mtm_receiving_application");
        var test = Service_SeedLedger.ScopeFor("172.16.1.104", 3306, "mtm_receiving_application_test");
        var fingerprint = Service_SeedLedger.Fingerprint("INSERT INTO t VALUES (1);");

        ledger.Record(live, "SeedData/01_seed_volvo_data.sql", fingerprint);

        ledger.WasApplied(test, "SeedData/01_seed_volvo_data.sql", fingerprint)
            .Should()
            .BeFalse("seeding the live database says nothing about the test one");
    }

    [Fact]
    public void ScopeFor_ShouldIgnoreCaseAndSurroundingSpace_WhenBuildingTheDatabaseKey()
    {
        Service_SeedLedger.ScopeFor(" 172.16.1.104 ", 3306, " MTM_Receiving_Application ")
            .Should()
            .Be(Service_SeedLedger.ScopeFor("172.16.1.104", 3306, "mtm_receiving_application"));
    }

    [Fact]
    public void Fingerprint_ShouldMatch_WhenOnlyTheLineEndingsDiffer()
    {
        Service_SeedLedger.Fingerprint("INSERT INTO t\r\nVALUES (1);\r\n")
            .Should()
            .Be(Service_SeedLedger.Fingerprint("INSERT INTO t\nVALUES (1);\n"));
    }

    [Fact]
    public void Fingerprint_ShouldDiffer_WhenASeedGainsARow()
    {
        Service_SeedLedger.Fingerprint("INSERT INTO t VALUES (1);")
            .Should()
            .NotBe(Service_SeedLedger.Fingerprint("INSERT INTO t VALUES (1), (2);"));
    }

    [Fact]
    public void Clone_ShouldKeepTheRecordedSeeds_WhenSettingsAreCopiedBeforeSaving()
    {
        var settings = new Model_ConnectionSettings();
        var scope = Service_SeedLedger.ScopeFor("172.16.1.104", 3306, "mtm_receiving_application");
        var ledger = new Service_SeedLedger(settings);
        var fingerprint = Service_SeedLedger.Fingerprint("INSERT INTO t VALUES (1);");

        ledger.Record(scope, "SeedData/01_seed_volvo_data.sql", fingerprint);
        var copy = settings.Clone();

        new Service_SeedLedger(copy)
            .WasApplied(scope, "SeedData/01_seed_volvo_data.sql", fingerprint)
            .Should()
            .BeTrue("the settings file is written from a clone");
    }
}
