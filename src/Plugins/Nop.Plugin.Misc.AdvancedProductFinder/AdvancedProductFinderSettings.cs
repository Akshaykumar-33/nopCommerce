using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.AdvancedProductFinder;

/// <summary>
/// Represents Advanced Product Finder settings
/// </summary>
public class AdvancedProductFinderSettings : ISettings
{
    /// <summary>
    /// Gets or sets a value indicating whether the public finder page is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the homepage widget CTA is shown
    /// </summary>
    public bool ShowHomepageWidget { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether recently viewed products are shown on the finder page
    /// </summary>
    public bool ShowRecentlyViewed { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether complete-the-set suggestions are shown
    /// </summary>
    public bool ShowCompleteTheSet { get; set; } = true;

    /// <summary>
    /// Gets or sets the number of products per page on the finder
    /// </summary>
    public int ProductsPerPage { get; set; } = 12;

    /// <summary>
    /// Gets or sets the number of complete-the-set products to show
    /// </summary>
    public int CompleteTheSetNumber { get; set; } = 4;

    /// <summary>
    /// Gets or sets the widget zone for the homepage CTA
    /// </summary>
    public string WidgetZone { get; set; } = "home_page_before_products";
}
