using Nop.Plugin.Misc.AdvancedProductFinder.Models;

namespace Nop.Plugin.Misc.AdvancedProductFinder.Factories;

/// <summary>
/// Represents Advanced Product Finder model factory
/// </summary>
public interface IAdvancedProductFinderModelFactory
{
    /// <summary>
    /// Prepare the finder page model
    /// </summary>
    Task<FinderModel> PrepareFinderModelAsync(FinderModel model);

    /// <summary>
    /// Prepare product results for the current filters
    /// </summary>
    Task<FinderProductsModel> PrepareFinderProductsModelAsync(FinderModel model);

    /// <summary>
    /// Prepare the homepage widget model
    /// </summary>
    Task<FinderWidgetModel> PrepareWidgetModelAsync();
}
