using MTM_Receiving_Application.Module_Dunnage.Helpers;

namespace MTM_Receiving_Application.Tests.Unit.Module_Dunnage;

/// <summary>
/// Groups the tests that move <see cref="Helper_DunnageImagePaths"/>'s root folders.
/// </summary>
/// <remarks>
/// Those roots are process-wide statics, so two classes that set them in parallel rewrite each other's
/// paths and point a file write at a directory the other test has already deleted. Sharing one
/// non-parallel collection keeps them apart without changing the helper, which the app needs to stay
/// static because the views read it directly.
/// </remarks>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class DunnageImagePathCollection
{
    public const string Name = "Dunnage image paths";
}
