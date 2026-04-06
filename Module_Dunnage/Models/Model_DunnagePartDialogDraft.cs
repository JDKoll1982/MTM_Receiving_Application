using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Dunnage.Models;

public class Model_DunnagePartDialogDraft
{
    public string PartId { get; set; } = string.Empty;

    public string ImagePath { get; set; } = string.Empty;

    public string HomeLocation { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string SelectedInventoryMethod { get; set; } = "Not Inventoried";

    public Dictionary<string, object?> SpecValues { get; set; } = new();

    public Model_DunnagePartDialogDraft Clone()
    {
        return new Model_DunnagePartDialogDraft
        {
            PartId = PartId,
            ImagePath = ImagePath,
            HomeLocation = HomeLocation,
            Notes = Notes,
            SelectedInventoryMethod = SelectedInventoryMethod,
            SpecValues = new Dictionary<string, object?>(SpecValues),
        };
    }
}
