using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MTM_Receiving_Application.Infrastructure.DependencyInjection;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Scanner.Services;
using MTM_Receiving_Application.Module_Scanner.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_Scanner.Services;

public sealed class ServiceCollection_ScannerWiringTests
{
    [Fact]
    public void AddModuleServices_ShouldRegisterScannerInterfacesWithExpectedImplementations()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration();

        services.AddModuleServices(configuration);

        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IService_ScannerNavigation)
            && descriptor.ImplementationType == typeof(Service_ScannerNavigation)
            && descriptor.Lifetime == ServiceLifetime.Singleton);

        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IService_ScannerWorkflow)
            && descriptor.ImplementationType == typeof(Service_ScannerWorkflow)
            && descriptor.Lifetime == ServiceLifetime.Singleton);

        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(IService_ScannerValidation)
            && descriptor.ImplementationType == typeof(Service_ScannerValidation)
            && descriptor.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddModuleServices_ShouldRegisterScannerViewModelsAsTransient()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration();

        services.AddModuleServices(configuration);

        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(ViewModel_Scanner_Main)
            && descriptor.Lifetime == ServiceLifetime.Transient);

        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(ViewModel_Scanner_Workbench)
            && descriptor.Lifetime == ServiceLifetime.Transient);

        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(ViewModel_Scanner_History)
            && descriptor.Lifetime == ServiceLifetime.Transient);

        services.Should().Contain(descriptor =>
            descriptor.ServiceType == typeof(ViewModel_Scanner_Settings)
            && descriptor.Lifetime == ServiceLifetime.Transient);
    }

    private static IConfiguration BuildConfiguration()
    {
        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:MySql"] =
                "Server=localhost;Port=3306;Database=mtm_receiving_application;Uid=test;Pwd=test;",
            ["ConnectionStrings:InforVisual"] =
                "Server=localhost;Database=mtmfg;Trusted_Connection=True;ApplicationIntent=ReadOnly;",
        };

        return new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
    }
}
