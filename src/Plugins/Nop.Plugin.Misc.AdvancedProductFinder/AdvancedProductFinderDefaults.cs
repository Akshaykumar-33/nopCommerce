namespace Nop.Plugin.Misc.AdvancedProductFinder;

/// <summary>
/// Represents plugin constants
/// </summary>
public static class AdvancedProductFinderDefaults
{
    /// <summary>
    /// Gets the plugin system name
    /// </summary>
    public static string SystemName => "Misc.AdvancedProductFinder";

    /// <summary>
    /// Gets the configuration route name
    /// </summary>
    public static string ConfigurationRouteName => "Plugin.Misc.AdvancedProductFinder.Configure";

    /// <summary>
    /// Gets the public finder route name
    /// </summary>
    public static string FinderRouteName => "Plugin.Misc.AdvancedProductFinder.Finder";

    /// <summary>
    /// Gets the AJAX products route name
    /// </summary>
    public static string GetProductsRouteName => "Plugin.Misc.AdvancedProductFinder.GetProducts";
}
