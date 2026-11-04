using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.AdvancedProductFinder.Models;

/// <summary>
/// Represents a plugin configuration model
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugins.Misc.AdvancedProductFinder.Fields.Enabled")]
    public bool Enabled { get; set; }
    public bool Enabled_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.AdvancedProductFinder.Fields.ShowHomepageWidget")]
    public bool ShowHomepageWidget { get; set; }
    public bool ShowHomepageWidget_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.AdvancedProductFinder.Fields.ShowRecentlyViewed")]
    public bool ShowRecentlyViewed { get; set; }
    public bool ShowRecentlyViewed_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.AdvancedProductFinder.Fields.ShowCompleteTheSet")]
    public bool ShowCompleteTheSet { get; set; }
    public bool ShowCompleteTheSet_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.AdvancedProductFinder.Fields.ProductsPerPage")]
    public int ProductsPerPage { get; set; }
    public bool ProductsPerPage_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.AdvancedProductFinder.Fields.CompleteTheSetNumber")]
    public int CompleteTheSetNumber { get; set; }
    public bool CompleteTheSetNumber_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.AdvancedProductFinder.Fields.WidgetZone")]
    public string WidgetZone { get; set; }
    public bool WidgetZone_OverrideForStore { get; set; }
}
