using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Volvo.Models;

public class Model_VolvoPartNumberEditDialog
{
    public string CurrentPartNumber { get; set; } = string.Empty;

    public List<Model_VolvoPart> AvailableParts { get; set; } = new();

    public HashSet<string> ExistingPartNumbers { get; set; } =
        new(System.StringComparer.OrdinalIgnoreCase);
}
