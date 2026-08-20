using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Scanner.Helpers;

namespace MTM_Receiving_Application.Tests.Unit.Module_Scanner.Helpers;

public sealed class Helper_ScannerAccessTests
{
    [Theory]
    [InlineData("John Koll", "MANT\\jkoll", "IT", true)] // Full name match
    [InlineData("john koll", "MANT\\jkoll", "IT", true)] // Case-insensitive full name
    [InlineData("John Koll", "MANT\\johnk", "IT", true)] // Username match
    [InlineData("Someone Else", "MANT\\jkoll", "IT", true)] // Username match regardless of full name
    [InlineData("Someone Else", "johnk", "IT", true)] // Bare username (no domain)
    [InlineData("Someone Else", "MANT\\jsmith", "Developer", true)] // Department developer
    [InlineData("Someone Else", "MANT\\jsmith", "IT", false)] // Not a developer
    public void IsDeveloperUser_ShouldMatch_WhenUserMatchesDeveloperProfile(
        string fullName,
        string windowsUsername,
        string department,
        bool expected
    )
    {
        var user = new Model_User
        {
            FullName = fullName,
            WindowsUsername = windowsUsername,
            Department = department,
        };

        Helper_ScannerAccess.IsDeveloperUser(user).Should().Be(expected);
    }

    [Theory]
    [InlineData("jkoll", true)]
    [InlineData("johnk", true)]
    [InlineData("MANT\\jkoll", true)]
    [InlineData("someone", false)]
    [InlineData(null, false)]
    [InlineData("", false)]
    public void IsDeveloperUser_ShouldUseFallbackUserName_WhenUserIsNull(string? fallback, bool expected)
    {
        Helper_ScannerAccess.IsDeveloperUser(user: null, fallback).Should().Be(expected);
    }

    [Fact]
    public void IsDeveloperUser_ShouldBeFalse_ForNullUserAndEmptyFallback()
    {
        Helper_ScannerAccess.IsDeveloperUser(user: null, null).Should().BeFalse();
    }
}
