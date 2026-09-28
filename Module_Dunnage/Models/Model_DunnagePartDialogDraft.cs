using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Dunnage.Models;

public class Model_DunnagePartDialogDraft
{
    public string PartId { get; set; } = string.Empty;

    public string ImagePath { get; set; } = string.Empty;

    public string HomeLocation { get; set; } = string.Empty;

    public string QuantityType { get; set; } = "Quantity";

    public string SelectedQuantityType { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string SelectedInventoryMethod { get; set; } = "Not Inventoried";

    public string? Udc1 { get; set; }
    public string? Udc2 { get; set; }
    public string? Udc3 { get; set; }
    public string? Udc4 { get; set; }
    public string? Udc5 { get; set; }
    public string? Udc6 { get; set; }
    public string? Udc7 { get; set; }
    public string? Udc8 { get; set; }
    public string? Udc9 { get; set; }
    public string? Udc10 { get; set; }

    public string? GetUdcValue(int slot) =>
        slot switch
        {
            1 => Udc1,
            2 => Udc2,
            3 => Udc3,
            4 => Udc4,
            5 => Udc5,
            6 => Udc6,
            7 => Udc7,
            8 => Udc8,
            9 => Udc9,
            10 => Udc10,
            _ => null,
        };

    public void SetUdcValue(int slot, string? value)
    {
        switch (slot)
        {
            case 1:
                Udc1 = value;
                break;
            case 2:
                Udc2 = value;
                break;
            case 3:
                Udc3 = value;
                break;
            case 4:
                Udc4 = value;
                break;
            case 5:
                Udc5 = value;
                break;
            case 6:
                Udc6 = value;
                break;
            case 7:
                Udc7 = value;
                break;
            case 8:
                Udc8 = value;
                break;
            case 9:
                Udc9 = value;
                break;
            case 10:
                Udc10 = value;
                break;
        }
    }

    public Model_DunnagePartDialogDraft Clone()
    {
        return new Model_DunnagePartDialogDraft
        {
            PartId = PartId,
            ImagePath = ImagePath,
            HomeLocation = HomeLocation,
            QuantityType = QuantityType,
            SelectedQuantityType = SelectedQuantityType,
            Notes = Notes,
            SelectedInventoryMethod = SelectedInventoryMethod,
            Udc1 = Udc1,
            Udc2 = Udc2,
            Udc3 = Udc3,
            Udc4 = Udc4,
            Udc5 = Udc5,
            Udc6 = Udc6,
            Udc7 = Udc7,
            Udc8 = Udc8,
            Udc9 = Udc9,
            Udc10 = Udc10,
        };
    }
}
