using FluentAssertions;
using MTM_Receiving_Application.Module_Settings.Core.Helpers;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Tests.Unit.Module_Settings.Core.Helpers;

public sealed class Helper_EmailRecipientFormattingTests
{
    [Fact]
    public void GenerateDefaultEmail_ShouldUseFirstInitialAndLastName()
    {
        var email = Helper_EmailRecipientFormatting.GenerateDefaultEmail("Michelle", "Laurin");

        email.Should().Be("mlaurin@mantoolmfg.com");
    }

    [Fact]
    public void BuildFormattedRecipientList_ShouldJoinRecipientsWithoutTrailingSemicolon()
    {
        var recipients = new List<Model_EmailRecipientSetting>
        {
            new()
            {
                Id = 1,
                FirstName = "Michelle",
                LastName = "Laurin",
                RecipientType = "To",
                Email = "mlaurin@mantoolmfg.com",
            },
            new()
            {
                Id = 2,
                FirstName = "Charles",
                LastName = "Ehlenbeck",
                RecipientType = "To",
                Email = "CEhlenbeck@mantoolmfg.com",
            },
        };

        var formatted = Helper_EmailRecipientFormatting.BuildFormattedRecipientList(
            recipients,
            "To"
        );

        formatted
            .Should()
            .Be(
                "\"Michelle Laurin\" <mlaurin@mantoolmfg.com>; \"Charles Ehlenbeck\" <CEhlenbeck@mantoolmfg.com>"
            );
    }
}
