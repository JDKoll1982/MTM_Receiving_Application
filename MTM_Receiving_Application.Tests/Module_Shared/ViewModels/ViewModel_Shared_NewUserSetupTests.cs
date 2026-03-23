using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Shared.ViewModels;

public class ViewModel_Shared_NewUserSetupTests
{
    [Theory]
    [InlineData("john", "koll", "John Koll")]
    [InlineData("JOHN", "KOLL", "John Koll")]
    [InlineData("jOhN", "KoLL", "John Koll")]
    [InlineData("mary ann", "o'connor", "Mary Ann O'Connor")]
    public void FullName_ShouldNormalizeFirstAndLastName(
        string firstName,
        string lastName,
        string expected
    )
    {
        var viewModel = CreateViewModel();

        viewModel.FirstName = firstName;
        viewModel.LastName = lastName;

        viewModel.FullName.Should().Be(expected);
    }

    [Fact]
    public async Task CreateAccountAsync_ShouldSendFormattedFullNameToAuthenticationService()
    {
        var authServiceMock = new Mock<IService_Authentication>();
        Model_User? capturedUser = null;

        authServiceMock
            .Setup(service => service.ValidatePinAsync("1234", 42))
            .ReturnsAsync(Model_ValidationResult.Valid());

        authServiceMock
            .Setup(service =>
                service.CreateNewUserAsync(
                    It.IsAny<Model_User>(),
                    It.IsAny<string>(),
                    It.IsAny<IProgress<string>>()
                )
            )
            .Callback<Model_User, string, IProgress<string>?>((user, _, _) => capturedUser = user)
            .ReturnsAsync(Model_CreateUserResult.SuccessResult(42));

        var viewModel = CreateViewModel(authServiceMock);
        viewModel.EmployeeNumber = "42";
        viewModel.WindowsUsername = "jkoll";
        viewModel.FirstName = "jOhN";
        viewModel.LastName = "KoLL";
        viewModel.Department = "IT";
        viewModel.Shift = "1st";
        viewModel.Pin = "1234";
        viewModel.CreatedBy = "admin";

        var result = await viewModel.CreateAccountAsync();

        result.Should().BeTrue();
        capturedUser.Should().NotBeNull();
        capturedUser!.FullName.Should().Be("John Koll");
        capturedUser.EmployeeNumber.Should().Be(42);
    }

    private static ViewModel_Shared_NewUserSetup CreateViewModel(
        Mock<IService_Authentication>? authServiceMock = null
    )
    {
        authServiceMock ??= new Mock<IService_Authentication>();

        return new ViewModel_Shared_NewUserSetup(
            authServiceMock.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }
}
