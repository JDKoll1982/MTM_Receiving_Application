using MTM_Waitlist_Application_2._0.Core;
using System.Reflection;
using System.Windows.Controls;
using TextBox = System.Windows.Controls.TextBox;

// Encapsulation Complete. (2024/07/30)

namespace MTM_Waitlist_Application_2._0
{
    public class AutoCompleteComboBox : System.Windows.Controls.ComboBox
    {
        private TextBox? _editableTextBox;

        public AutoCompleteComboBox(TextBox? editableTextBox)
        {
            try
            {
                _editableTextBox = editableTextBox;
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }


        public override void OnApplyTemplate()
        {
            try
            {
                base.OnApplyTemplate();

                // Find the editable text box in the template
                _editableTextBox = this.Template.FindName("PART_EditableTextBox", this) as System.Windows.Controls.TextBox;
                if (_editableTextBox != null)
                {
                    // Attach the text changed event handler
                    _editableTextBox.TextChanged += EditableTextBox_TextChanged;
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }


        private void EditableTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (_editableTextBox != null)
                {
                    string text = _editableTextBox.Text;
                    if (string.IsNullOrEmpty(text))
                    {
                        this.IsDropDownOpen = false;
                        return;
                    }

                    this.IsDropDownOpen = true;

                    // Iterate through items to find a match
                    foreach (var item in this.Items)
                    {
                        if (item.ToString()!.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                        {
                            this.SelectedItem = item;
                            _editableTextBox.SelectionStart = text.Length;
                            _editableTextBox.SelectionLength = item.ToString().Length - text.Length;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

    }
}

