using MTM_Database_Deployment.Models;

namespace MTM_Database_Deployment.Services;

/// <summary>
/// Discovers every SQL object script that the deployment ships.
/// </summary>
public sealed class Service_ObjectCatalog
{
    private readonly string _sqlRoot;

    public Service_ObjectCatalog(string sqlRoot)
    {
        _sqlRoot = sqlRoot;
    }

    public string SqlRoot => _sqlRoot;

    public IReadOnlyList<Model_SqlObject> LoadObjects()
    {
        var objects = new List<Model_SqlObject>();
        if (!Directory.Exists(_sqlRoot))
        {
            return objects;
        }

        objects.AddRange(LoadFolder("Schemas", Enum_SqlObjectKind.Schema, recurse: false));
        objects.AddRange(LoadFolder("Views", Enum_SqlObjectKind.View, recurse: false));
        objects.AddRange(LoadFolder("StoredProcedures", Enum_SqlObjectKind.Procedure, recurse: true));
        objects.AddRange(LoadFolder("Triggers", Enum_SqlObjectKind.Trigger, recurse: false));
        objects.AddRange(LoadFolder("SeedData", Enum_SqlObjectKind.Seed, recurse: false));

        return objects
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.RelativePath, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private IEnumerable<Model_SqlObject> LoadFolder(string folderName, Enum_SqlObjectKind kind, bool recurse)
    {
        var folderPath = Path.Combine(_sqlRoot, folderName);
        if (!Directory.Exists(folderPath))
        {
            return Array.Empty<Model_SqlObject>();
        }

        return Directory
            .EnumerateFiles(folderPath, "*.sql", recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}_Archive{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .Select(path => new Model_SqlObject
            {
                Name = Path.GetFileNameWithoutExtension(path),
                FullPath = path,
                RelativePath = Path.GetRelativePath(_sqlRoot, path).Replace('\\', '/'),
                Kind = kind
            })
            .ToList();
    }
}
