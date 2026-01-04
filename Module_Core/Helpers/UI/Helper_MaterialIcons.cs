using System;
using System.Collections.Generic;
using System.Linq;
using Material.Icons;

namespace MTM_Receiving_Application.Module_Core.Helpers.UI;

public static class Helper_MaterialIcons
{
    private static List<MaterialIconKind>? _allIcons;

    public static List<MaterialIconKind> GetAllIcons()
    {
        if (_allIcons == null)
        {
            _allIcons = Enum.GetValues<MaterialIconKind>().ToList();
        }
        return _allIcons;
    }

    public static IEnumerable<MaterialIconKind> SearchIcons(string searchText)
    {
        var all = GetAllIcons();
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return all;
        }

        searchText = searchText.ToLower().Trim();
        return all.Where(k => k.ToString().ToLower().Contains(searchText));
    }
}
