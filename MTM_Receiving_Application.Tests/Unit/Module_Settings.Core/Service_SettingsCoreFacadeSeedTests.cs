using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Moq;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Core.Enums;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Settings.Core.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Settings.Core;

/// <summary>
/// Covers the startup seeding pass. It has to resolve both scopes — not just the user's own rows — because
/// the screens read their wording one key at a time while the main window comes up, and a cold key costs a
/// database round trip each. Getting this wrong does not fail a build; it shows up as a blank window.
/// </summary>
public sealed class Service_SettingsCoreFacadeSeedTests
{
    private static Model_SettingsDefinition Define(
        string category,
        string key,
        Enum_SettingsScope scope
    ) =>
        new()
        {
            Category = category,
            Key = key,
            DisplayName = key,
            DataType = Enum_SettingsDataType.String,
            Scope = scope,
        };

    private static (
        Service_SettingsCoreFacade Facade,
        Mock<IMediator> Mediator,
        List<GetSettingQuery> Sent,
        Mock<ISettingsRowSnapshot> Snapshot
    ) Build(params Model_SettingsDefinition[] definitions)
    {
        var registry = new Mock<ISettingsMetadataRegistry>();
        registry.Setup(x => x.GetAll()).Returns(definitions);

        var snapshot = new Mock<ISettingsRowSnapshot>();
        snapshot.SetupGet(x => x.IsPrimed).Returns(true);

        var sent = new List<GetSettingQuery>();
        var mediator = new Mock<IMediator>();

        mediator
            .Setup(x =>
                x.Send(It.IsAny<GetSettingQuery>(), It.IsAny<CancellationToken>())
            )
            .Callback<IRequest<Model_Dao_Result<Model_SettingsValue>>, CancellationToken>(
                (request, _) =>
                {
                    lock (sent)
                    {
                        sent.Add((GetSettingQuery)request);
                    }
                }
            )
            .ReturnsAsync((IRequest<Model_Dao_Result<Model_SettingsValue>> request, CancellationToken _) =>
                Model_Dao_Result_Factory.Success(
                    new Model_SettingsValue { Category = "TEST", Key = "TEST" }
                )
            );

        return (
            new Service_SettingsCoreFacade(mediator.Object, registry.Object, snapshot.Object),
            mediator,
            sent,
            snapshot
        );
    }

    [Fact]
    public async Task InitializeDefaultsAsync_ShouldResolveBothScopes_WhenAUserIsGiven()
    {
        var (facade, _, sent, _) = Build(
            Define("TEST", "TEST.User.A", Enum_SettingsScope.User),
            Define("TEST", "TEST.System.B", Enum_SettingsScope.System),
            Define("TEST", "TEST.System.C", Enum_SettingsScope.System)
        );

        await facade.InitializeDefaultsAsync(6229);

        sent.Should().HaveCount(3, "every definition is resolved so the cache is warm for the screens");
        sent.Single(query => query.Key == "TEST.User.A")
            .UserId.Should()
            .Be(6229, "a user-scoped row is read for the signed-in user");
        sent.Where(query => query.Key != "TEST.User.A")
            .Should()
            .OnlyContain(
                query => query.UserId == null,
                "a system-scoped row is shared, so it is never read as one user's"
            );
    }

    [Fact]
    public async Task InitializeDefaultsAsync_ShouldSkipUserScopedSettings_WhenNoUserIsGiven()
    {
        var (facade, _, sent, _) = Build(
            Define("TEST", "TEST.User.A", Enum_SettingsScope.User),
            Define("TEST", "TEST.System.B", Enum_SettingsScope.System)
        );

        await facade.InitializeDefaultsAsync();

        sent.Should().ContainSingle().Which.Key.Should().Be("TEST.System.B");
    }

    [Fact]
    public async Task InitializeDefaultsAsync_ShouldCountEveryDefinition_WhenReportingProgress()
    {
        var reports = new List<Model_SettingsSeedProgress>();

        var (facade, _, _, _) = Build(
            Define("TEST", "TEST.System.A", Enum_SettingsScope.System),
            Define("TEST", "TEST.System.B", Enum_SettingsScope.System),
            Define("TEST", "TEST.System.C", Enum_SettingsScope.System)
        );

        await facade.InitializeDefaultsAsync(
            progress: new Progress<Model_SettingsSeedProgress>(report =>
            {
                lock (reports)
                {
                    reports.Add(report);
                }
            })
        );

        // Progress<T> posts to the captured context; give the queued callbacks a moment to run.
        await Task.Delay(50);

        lock (reports)
        {
            reports.Should().NotBeEmpty();
            reports.Should().OnlyContain(report => report.Total == 3);
            reports.Max(report => report.Completed).Should().Be(3);
        }
    }

    [Fact]
    public async Task InitializeDefaultsAsync_ShouldPrimeTheSnapshotOnce_ForEachCategoryInPlay()
    {
        var (facade, _, _, snapshot) = Build(
            Define("TEST", "TEST.System.A", Enum_SettingsScope.System),
            Define("TEST", "TEST.System.B", Enum_SettingsScope.System),
            Define("OTHER", "OTHER.User.A", Enum_SettingsScope.User)
        );

        await facade.InitializeDefaultsAsync(6229);

        snapshot.Verify(
            x =>
                x.PrimeAsync(
                    6229,
                    It.Is<IReadOnlyCollection<string>>(categories =>
                        categories.Count == 2
                        && categories.Contains("TEST")
                        && categories.Contains("OTHER")
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once,
            "one read per category is what stops the per-setting round trips"
        );
    }

    [Fact]
    public async Task InitializeDefaultsAsync_ShouldDoNothing_WhenNoSettingsAreRegistered()
    {
        var (facade, mediator, _, _) = Build();

        await facade.InitializeDefaultsAsync(6229);

        mediator.Verify(
            x => x.Send(It.IsAny<GetSettingQuery>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task InitializeDefaultsAsync_ShouldStillResolveTheRest_WhenOneSettingFails()
    {
        var registry = new Mock<ISettingsMetadataRegistry>();
        registry
            .Setup(x => x.GetAll())
            .Returns(
                [
                    Define("TEST", "TEST.System.A", Enum_SettingsScope.System),
                    Define("TEST", "TEST.System.B", Enum_SettingsScope.System),
                ]
            );

        var attempted = new List<string>();
        var mediator = new Mock<IMediator>();

        mediator
            .Setup(x => x.Send(It.IsAny<GetSettingQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Model_Dao_Result<Model_SettingsValue>>, CancellationToken>(
                (request, _) =>
                {
                    lock (attempted)
                    {
                        attempted.Add(((GetSettingQuery)request).Key);
                    }
                }
            )
            .ReturnsAsync((IRequest<Model_Dao_Result<Model_SettingsValue>> request, CancellationToken _) =>
            {
                var query = (GetSettingQuery)request;

                return query.Key == "TEST.System.A"
                    ? Model_Dao_Result_Factory.Failure<Model_SettingsValue>("TEST failure")
                    : Model_Dao_Result_Factory.Success(
                        new Model_SettingsValue { Category = "TEST", Key = query.Key }
                    );
            });

        var facade = new Service_SettingsCoreFacade(
            mediator.Object,
            registry.Object,
            Mock.Of<ISettingsRowSnapshot>()
        );

        await facade.InitializeDefaultsAsync(6229);

        attempted.Should().BeEquivalentTo(["TEST.System.A", "TEST.System.B"]);
    }
}
