using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.AdvancedProductFinder.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.AdvancedProductFinder.Components;

/// <summary>
/// Represents the homepage Advanced Product Finder widget
/// </summary>
public class AdvancedProductFinderWidgetViewComponent : NopViewComponent
{
    #region Fields

    protected readonly AdvancedProductFinderSettings _settings;
    protected readonly IAdvancedProductFinderModelFactory _finderModelFactory;

    #endregion

    #region Ctor

    public AdvancedProductFinderWidgetViewComponent(
        AdvancedProductFinderSettings settings,
        IAdvancedProductFinderModelFactory finderModelFactory)
    {
        _settings = settings;
        _finderModelFactory = finderModelFactory;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Invoke the widget view component
    /// </summary>
    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        if (!_settings.Enabled || !_settings.ShowHomepageWidget)
            return Content(string.Empty);

        var model = await _finderModelFactory.PrepareWidgetModelAsync();

        return await ViewAsync("~/Plugins/Misc.AdvancedProductFinder/Views/PublicInfo.cshtml", model);
    }

    #endregion
}
