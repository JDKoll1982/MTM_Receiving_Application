using System;
using System.Collections.Generic;
using System.Linq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;

namespace MTM_Receiving_Application.Module_Core.Services.UI
{
    public class Service_ViewModelRegistry : IService_ViewModelRegistry
    {
        private readonly List<WeakReference<object>> _viewModels = new();

        public void Register(object viewModel)
        {
            _viewModels.Add(new WeakReference<object>(viewModel));
            Cleanup();
        }

        public IEnumerable<T> GetViewModels<T>()
        {
            Cleanup();
            return _viewModels
                .Select(r => r.TryGetTarget(out var target) ? target : null)
                .Where(t => t != null && t is T)
                .Cast<T>();
        }

        public void ClearAllInputs()
        {
            Cleanup();
            foreach (var vmRef in _viewModels)
            {
                if (vmRef.TryGetTarget(out var vm))
                {
                    if (vm is IResettableViewModel resettable)
                    {
                        resettable.ResetToDefaults();
                    }
                }
            }
        }

        private void Cleanup()
        {
            _viewModels.RemoveAll(r => !r.TryGetTarget(out _));
        }
    }
}
