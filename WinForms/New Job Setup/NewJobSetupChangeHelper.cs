using System.Collections.ObjectModel;
using System.Reflection;
using MTM_Waitlist_Application_2._0.Core;
using MTM_Waitlist_Application_2._0.Core.Database_Classes;

namespace MTM_Waitlist_Application_2._0.WinForms.New_Job_Setup
{
    public static class NewJobSetupChangeHelper
    {
        public static void ChangeOperationComboBox(ComboBox newJobSetupComboBoxOp,
            ComboBox newJobSetupComboBoxPartId, TextBox debugTextBox)
        {
            try
            {
                EnableComboBox(newJobSetupComboBoxOp);

                var operations = ComboBoxHelper.GetOperationsForPartId(newJobSetupComboBoxPartId.Text);

                if (operations.Count == 0)
                {
                    newJobSetupComboBoxOp.DataSource = null;
                    newJobSetupComboBoxOp.Items.Clear();
                    newJobSetupComboBoxOp.Items.Add("No Operations Found");
                    DisableComboBox(newJobSetupComboBoxOp);
                    newJobSetupComboBoxOp.SelectedIndex = 0;
                }
                else
                {
                    newJobSetupComboBoxOp.DataSource = operations;
                    debugTextBox.Text = operations.Count.ToString();
                    if (newJobSetupComboBoxOp.Items.Count == 1)
                    {
                        DisableComboBox(newJobSetupComboBoxOp);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        public static void ChangeDieComboBox(ComboBox newJobSetupComboBoxDie, ComboBox newJobSetupComboBoxPartId,
            ComboBox newJobSetupComboBoxOp, TextBox newJobSetupTextBoxDieLocation)
        {
            try
            {
                var dataSource =
                    ComboBoxHelper.GetFilteredDie(newJobSetupComboBoxPartId.Text, newJobSetupComboBoxOp.Text);

                if (dataSource.Count == 0)
                {
                    newJobSetupComboBoxDie.DataSource = null;
                    newJobSetupComboBoxDie.Items.Clear();
                    newJobSetupComboBoxDie.Items.Add("No Dies Found");
                    newJobSetupTextBoxDieLocation.Text = @"No Die Selected";
                    DisableComboBox(newJobSetupComboBoxDie);
                    newJobSetupComboBoxDie.SelectedIndex = 0;
                }
                else
                {
                    newJobSetupComboBoxDie.DataSource = dataSource;
                    ChangeDieLocationTextBox(newJobSetupComboBoxDie, newJobSetupTextBoxDieLocation);
                    if (newJobSetupComboBoxDie.Items.Count == 1)
                    {
                        DisableComboBox(newJobSetupComboBoxDie);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        public static void ChangeCoilComboBox(ComboBox newJobSetupComboBoxCoil, ComboBox newJobSetupComboBoxPartId,
            ComboBox newJobSetupComboBoxOp, ComboBox newJobSetupComboBoxDie, Label newJobSetupLabelCoil)
        {
            try
            {
                ObservableCollection<string> coils = ComboBoxHelper.GetFilteredCoils(newJobSetupComboBoxPartId.Text,
                    newJobSetupComboBoxOp.Text);

                if (coils.Count == 0)
                {
                    newJobSetupComboBoxCoil.DataSource = null;
                    newJobSetupComboBoxCoil.Items.Clear();
                    newJobSetupComboBoxCoil.Items.Add("No Coil Found");
                    DisableComboBox(newJobSetupComboBoxCoil);
                    newJobSetupComboBoxCoil.SelectedIndex = 0;
                }
                else
                {
                    newJobSetupComboBoxCoil.DataSource = coils;
                    if (newJobSetupComboBoxCoil.Items.Count == 1)
                    {
                        DisableComboBox(newJobSetupComboBoxDie);
                    }
                    else
                    {
                        EnableComboBox(newJobSetupComboBoxDie);
                    }
                }

                var coilLabelMappings = new Dictionary<string, string>
                {
                    { "MMC", "Coil" },
                    { "MMF", "Flat-stock" },
                    { "MMCCS", "Customer Supplied Coil" },
                    { "MMFCS", "Customer Supplied Flat-stock" },
                    { "MMR", "MMR (Not sure what this means)" },
                    { "MMS", "MMS (Not sure what this means)" }
                };

                foreach (var mapping in coilLabelMappings)
                {
                    if (newJobSetupComboBoxCoil.Text.Contains(mapping.Key))
                    {
                        newJobSetupLabelCoil.Text = mapping.Value;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        public static void ChangeContainerComboBox(ComboBox newJobSetupComboBoxContainer,
            ComboBox newJobSetupComboBoxPartId, ComboBox newJobSetupComboBoxOp)
        {
            try
            {
                if (newJobSetupComboBoxPartId.SelectedIndex == 0)
                {
                    DisableComboBox(newJobSetupComboBoxContainer);
                    return;
                }

                var containers =
                    ComboBoxHelper.GetFilteredContainer(newJobSetupComboBoxPartId.Text, newJobSetupComboBoxOp.Text);

                if (containers.Count == 0)
                {
                    return;
                }

                newJobSetupComboBoxContainer.Text = containers[0];
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        public static void ChangeSkidComboBox(ComboBox newJobSetupComboBoxSkid, ComboBox newJobSetupComboBoxPartId,
            ComboBox newJobSetupComboBoxOp)
        {
            try
            {
                if (newJobSetupComboBoxPartId.SelectedIndex == 0)
                {
                    DisableComboBox(newJobSetupComboBoxSkid);
                    return;
                }

                var skids = ComboBoxHelper.GetFilteredSkid(newJobSetupComboBoxPartId.Text,
                    newJobSetupComboBoxOp.Text);

                if (skids.Count == 0)
                {
                    return;
                }

                newJobSetupComboBoxSkid.Text = skids[0];
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        public static void ChangeCardboardComboBox(ComboBox newJobSetupComboBoxCardboard,
            ComboBox newJobSetupComboBoxPartId, ComboBox newJobSetupComboBoxOp)
        {
            try
            {
                if (newJobSetupComboBoxPartId.SelectedIndex == 0)
                {
                    DisableComboBox(newJobSetupComboBoxCardboard);
                    return;
                }

                var cardboards =
                    ComboBoxHelper.GetFilteredCardboard(newJobSetupComboBoxPartId.Text, newJobSetupComboBoxOp.Text);

                if (cardboards.Count == 0)
                {
                    return;
                }

                newJobSetupComboBoxCardboard.Text = cardboards[0];
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        public static void ChangeBoxesComboBox(ComboBox newJobSetupComboBoxBox, ComboBox newJobSetupComboBoxPartId,
            ComboBox newJobSetupComboBoxOp)
        {
            try
            {
                if (newJobSetupComboBoxPartId.SelectedIndex == 0)
                {
                    DisableComboBox(newJobSetupComboBoxBox);
                    return;
                }

                var boxes = ComboBoxHelper.GetFilteredBox(newJobSetupComboBoxPartId.Text,
                    newJobSetupComboBoxOp.Text);

                if (boxes.Count == 0)
                {
                    return;
                }

                newJobSetupComboBoxBox.Text = boxes[0];
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        public static void ChangeOtherComboBoxes(ComboBox newJobSetupComboBoxPartId, ComboBox newJobSetupComboBoxOp,
            ComboBox newJobSetupComboBoxOther1, ComboBox newJobSetupComboBoxOther2,
            ComboBox newJobSetupComboBoxOther3, ComboBox newJobSetupComboBoxOther4,
            ComboBox newJobSetupComboBoxOther5)
        {
            try
            {
                if (newJobSetupComboBoxPartId.SelectedIndex == 0)
                {
                    DisableComboBox(newJobSetupComboBoxOther1);
                    DisableComboBox(newJobSetupComboBoxOther2);
                    DisableComboBox(newJobSetupComboBoxOther3);
                    DisableComboBox(newJobSetupComboBoxOther4);
                    DisableComboBox(newJobSetupComboBoxOther5);
                    return;
                }

                var other1Items =
                    ComboBoxHelper.GetFilteredOther1(newJobSetupComboBoxPartId.Text, newJobSetupComboBoxOp.Text);
                var other2Items =
                    ComboBoxHelper.GetFilteredOther2(newJobSetupComboBoxPartId.Text, newJobSetupComboBoxOp.Text);
                var other3Items =
                    ComboBoxHelper.GetFilteredOther3(newJobSetupComboBoxPartId.Text, newJobSetupComboBoxOp.Text);
                var other4Items =
                    ComboBoxHelper.GetFilteredOther4(newJobSetupComboBoxPartId.Text, newJobSetupComboBoxOp.Text);
                var other5Items =
                    ComboBoxHelper.GetFilteredOther5(newJobSetupComboBoxPartId.Text, newJobSetupComboBoxOp.Text);

                UpdateOtherComboBox(newJobSetupComboBoxOther1, other1Items);
                UpdateOtherComboBox(newJobSetupComboBoxOther2, other2Items);
                UpdateOtherComboBox(newJobSetupComboBoxOther3, other3Items);
                UpdateOtherComboBox(newJobSetupComboBoxOther4, other4Items);
                UpdateOtherComboBox(newJobSetupComboBoxOther5, other5Items);
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }

        public static void ChangeRadioButtons(RadioButton newJobSetupRadioButtonWorkInProgress,
            RadioButton newJobSetupRadioButtonOutsideService, RadioButton newJobSetupRadioButtonFinishedGoods,
            ComboBox newJobSetupComboBoxPartId, ComboBox newJobSetupComboBoxOp)
        {
            newJobSetupRadioButtonWorkInProgress.Checked = false;
            newJobSetupRadioButtonOutsideService.Checked = false;
            newJobSetupRadioButtonFinishedGoods.Checked = false;

            var partType = ComboBoxHelper.GetPartType(newJobSetupComboBoxPartId.Text, newJobSetupComboBoxOp.Text);

            if (partType.Count == 0)
            {
                return;
            }

            if (partType[0] == newJobSetupRadioButtonFinishedGoods.Text)
            {
                newJobSetupRadioButtonFinishedGoods.Checked = true;
            }
            else if (partType[0] == newJobSetupRadioButtonOutsideService.Text)
            {
                newJobSetupRadioButtonOutsideService.Checked = true;
            }
            else if (partType[0] == newJobSetupRadioButtonWorkInProgress.Text)
            {
                newJobSetupRadioButtonWorkInProgress.Checked = true;
            }
        }

        private static void EnableComboBox(ComboBox comboBox)
        {
            comboBox.Enabled = true;
        }

        private static void DisableComboBox(ComboBox comboBox)
        {
            comboBox.Enabled = false;
        }

        private static void UpdateOtherComboBox(ComboBox comboBox, ObservableCollection<string> items)
        {
            if (items.Count == 0)
            {
                return;
            }

            comboBox.Text = items[0];
        }

        private static void ChangeDieLocationTextBox(ComboBox newJobSetupComboBoxDie, TextBox newJobSetupTextBoxDieLocation)
        {
            try
            {
                if (newJobSetupComboBoxDie.SelectedItem != null)
                {
                    string? selectedDie = newJobSetupComboBoxDie.SelectedItem.ToString();
                    string? dieLocation = ComboBoxHelper.GetDieLocation(selectedDie);

                    if (!string.IsNullOrEmpty(dieLocation))
                    {
                        newJobSetupTextBoxDieLocation.Text = dieLocation;
                    }
                    else
                    {
                        newJobSetupTextBoxDieLocation.Text = @"Location not found";
                    }
                }
                else
                {
                    newJobSetupTextBoxDieLocation.Text = @"No Die Selected";
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError(MethodBase.GetCurrentMethod()!.Name, ex.Message);
            }
        }
    }
}