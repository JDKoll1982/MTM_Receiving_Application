using System;
using System.Linq;
using Xunit;
using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Services.UI;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;

namespace MTM_Receiving_Application.Tests.Module_Core.Services.UI
{
    /// <summary>
    /// Unit tests for Service_ViewModelRegistry
    /// </summary>
    [Trait("Category", "Unit")]
    [Trait("Layer", "Service")]
    public class Service_ViewModelRegistry_Tests
    {
        private class TestViewModel : IResettableViewModel
        {
            public bool ResetCalled { get; private set; }
            public void ResetToDefaults() => ResetCalled = true;
        }

        private class AnotherViewModel { }

        [Fact]
        public void Register_and_GetViewModels_WorksCorrectly()
        {
            // Arrange
            var sut = new Service_ViewModelRegistry();
            var vm1 = new TestViewModel();
            var vm2 = new AnotherViewModel();

            // Act
            sut.Register(vm1);
            sut.Register(vm2);

            var retrieved = sut.GetViewModels<TestViewModel>().ToList();

            // Assert
            retrieved.Should().HaveCount(1);
            retrieved.First().Should().Be(vm1);
        }

        [Fact]
        public void ClearAllInputs_CallsResetOnResettableVMs()
        {
            // Arrange
            var sut = new Service_ViewModelRegistry();
            var vm1 = new TestViewModel();
            var vm2 = new AnotherViewModel(); // Not resettable

            sut.Register(vm1);
            sut.Register(vm2);

            // Act
            sut.ClearAllInputs();

            // Assert
            vm1.ResetCalled.Should().BeTrue();
        }

        [Fact]
        public void Cleanup_RemovesDeadReferences()
        {
             // Arrange
            var sut = new Service_ViewModelRegistry();
            
            void RegisterTemporaryVM()
            {
                var vm = new TestViewModel();
                sut.Register(vm);
            }

            RegisterTemporaryVM();
            
            // Force GC to collect the temporary VM
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Act
            // GetViewModels calls Cleanup internally
            var list = sut.GetViewModels<TestViewModel>().ToList();

            // Assert
            list.Should().BeEmpty();
        }
    }
}
