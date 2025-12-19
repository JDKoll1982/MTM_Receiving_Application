using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Receiving;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.WinUI.UI.Controls;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Views.Receiving
{
    public sealed partial class ReviewGridView : UserControl
    {
        public ReviewGridViewModel ViewModel { get; }

        public ReviewGridView()
        {
            ViewModel = App.GetService<ReviewGridViewModel>();
            this.InitializeComponent();
        }

        private void ReviewDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                if (e.Row.DataContext is Model_ReceivingLoad load)
                {
                    // We need to know which column was edited.
                    // The Column property gives us the column definition.
                    // We can check the Header or Binding, but Binding is not directly accessible easily on the base class.
                    // Let's use Header or Tag if set.
                    
                    var header = e.Column.Header?.ToString();
                    string propertyName = string.Empty;

                    if (header == "PO Number") propertyName = nameof(Model_ReceivingLoad.PoNumber);
                    else if (header == "Part ID") propertyName = nameof(Model_ReceivingLoad.PartID);
                    
                    if (!string.IsNullOrEmpty(propertyName))
                    {
                        // Defer the update to let the binding commit first
                        this.DispatcherQueue.TryEnqueue(() => 
                        {
                            ViewModel.HandleCascadingUpdate(load, propertyName);
                        });
                    }
                }
            }
        }
    }
}
