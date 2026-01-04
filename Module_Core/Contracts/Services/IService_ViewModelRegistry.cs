using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services
{
    public interface IService_ViewModelRegistry
    {
        public void Register(object viewModel);
        public IEnumerable<T> GetViewModels<T>();
        public void ClearAllInputs();
    }
}
