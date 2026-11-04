using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Plugin.Misc.AdvancedProductFinder.Components;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Misc.AdvancedProductFinder;

/// <summary>
/// Represents the Advanced Product Finder plugin
/// </summary>
public class AdvancedProductFinderPlugin : BasePlugin, IMiscPlugin, IWidgetPlugin
{
    #region Fields

    protected readonly AdvancedProductFinderSettings _advancedProductFinderSettings;
    protected readonly ILocalizationService _localizationService;
    protected readonly ISettingService _settingService;
    protected readonly IWebHelper _webHelper;
    protected readonly WidgetSettings _widgetSettings;

    #endregion

    #region Ctor

    public AdvancedProductFinderPlugin(
        AdvancedProductFinderSettings advancedProductFinderSettings,
        ILocalizationService localizationService,
        ISettingService settingService,
        IWebHelper webHelper,
        WidgetSettings widgetSettings)
    {
        _advancedProductFinderSettings = advancedProductFinderSettings;
        _localizationService = localizationService;
        _settingService = settingService;
        _webHelper = webHelper;
        _widgetSettings = widgetSettings;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/AdvancedProductFinder/Configure";
    }

    /// <inheritdoc />
    public Task<IList<string>> GetWidgetZonesAsync()
    {
        var zone = string.IsNullOrWhiteSpace(_advancedProductFinderSettings.WidgetZone)
            ? PublicWidgetZones.HomepageBeforeProducts
            : _advancedProductFinderSettings.WidgetZone;

        return Task.FromResult<IList<string>>([zone]);
    }

    /// <inheritdoc />
    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(AdvancedProductFinderWidgetViewComponent);
    }

    /// <inheritdoc />
    public override async Task InstallAsync()
    {
        await _settingService.SaveSettingAsync(new AdvancedProductFinderSettings());

        if (!_widgetSettings.ActiveWidgetSystemNames.Contains(AdvancedProductFinderDefaults.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Add(AdvancedProductFinderDefaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Misc.AdvancedProductFinder.Menu"] = "Advanced Product Finder",
            ["Plugins.Misc.AdvancedProductFinder.Title"] = "Advanced Product Finder",
            ["Plugins.Misc.AdvancedProductFinder.Subtitle"] = "Filter by category, brand, price, and visual attributes to find the right products faster.",
            ["Plugins.Misc.AdvancedProductFinder.Keywords"] = "Keywords",
            ["Plugins.Misc.AdvancedProductFinder.Category"] = "Category",
            ["Plugins.Misc.AdvancedProductFinder.Manufacturer"] = "Manufacturer",
            ["Plugins.Misc.AdvancedProductFinder.PriceFrom"] = "Price from",
            ["Plugins.Misc.AdvancedProductFinder.PriceTo"] = "Price to",
            ["Plugins.Misc.AdvancedProductFinder.SortBy"] = "Sort by",
            ["Plugins.Misc.AdvancedProductFinder.Specifications"] = "Specifications",
            ["Plugins.Misc.AdvancedProductFinder.Apply"] = "Find products",
            ["Plugins.Misc.AdvancedProductFinder.Reset"] = "Reset filters",
            ["Plugins.Misc.AdvancedProductFinder.Results"] = "Matching products",
            ["Plugins.Misc.AdvancedProductFinder.ResultsCount"] = "{0} products found",
            ["Plugins.Misc.AdvancedProductFinder.NoResults"] = "No products match your filters. Try broadening your search.",
            ["Plugins.Misc.AdvancedProductFinder.RecentlyViewed"] = "Recently viewed",
            ["Plugins.Misc.AdvancedProductFinder.CompleteTheSet"] = "Complete the set",
            ["Plugins.Misc.AdvancedProductFinder.Previous"] = "Previous",
            ["Plugins.Misc.AdvancedProductFinder.Next"] = "Next",
            ["Plugins.Misc.AdvancedProductFinder.Page"] = "Page {0} of {1}",
            ["Plugins.Misc.AdvancedProductFinder.Widget.Title"] = "Find products your way",
            ["Plugins.Misc.AdvancedProductFinder.Widget.Description"] = "Use visual filters, price ranges, and smart suggestions to discover the perfect match.",
            ["Plugins.Misc.AdvancedProductFinder.Widget.Button"] = "Open product finder",
            ["Plugins.Misc.AdvancedProductFinder.Fields.Enabled"] = "Enabled",
            ["Plugins.Misc.AdvancedProductFinder.Fields.Enabled.Hint"] = "Check to enable the Advanced Product Finder page.",
            ["Plugins.Misc.AdvancedProductFinder.Fields.ShowHomepageWidget"] = "Show homepage widget",
            ["Plugins.Misc.AdvancedProductFinder.Fields.ShowHomepageWidget.Hint"] = "Check to show a call-to-action widget on the homepage.",
            ["Plugins.Misc.AdvancedProductFinder.Fields.ShowRecentlyViewed"] = "Show recently viewed",
            ["Plugins.Misc.AdvancedProductFinder.Fields.ShowRecentlyViewed.Hint"] = "Check to show recently viewed products on the finder page.",
            ["Plugins.Misc.AdvancedProductFinder.Fields.ShowCompleteTheSet"] = "Show complete the set",
            ["Plugins.Misc.AdvancedProductFinder.Fields.ShowCompleteTheSet.Hint"] = "Check to show related or same-category suggestions.",
            ["Plugins.Misc.AdvancedProductFinder.Fields.ProductsPerPage"] = "Products per page",
            ["Plugins.Misc.AdvancedProductFinder.Fields.ProductsPerPage.Hint"] = "Set how many products are shown per page.",
            ["Plugins.Misc.AdvancedProductFinder.Fields.CompleteTheSetNumber"] = "Complete-the-set products",
            ["Plugins.Misc.AdvancedProductFinder.Fields.CompleteTheSetNumber.Hint"] = "Set how many complete-the-set products to display.",
            ["Plugins.Misc.AdvancedProductFinder.Fields.WidgetZone"] = "Widget zone",
            ["Plugins.Misc.AdvancedProductFinder.Fields.WidgetZone.Hint"] = "Enter the public widget zone name for the homepage CTA (for example home_page_before_products)."
        });

        await base.InstallAsync();
    }

    /// <inheritdoc />
    public override async Task UninstallAsync()
    {
        await _settingService.DeleteSettingAsync<AdvancedProductFinderSettings>();

        if (_widgetSettings.ActiveWidgetSystemNames.Contains(AdvancedProductFinderDefaults.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Remove(AdvancedProductFinderDefaults.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.AdvancedProductFinder");

        await base.UninstallAsync();
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public bool HideInWidgetList => true;

    #endregion
}
