using Microsoft.AspNetCore.Mvc;
using Nop.Core.Http;
using Nop.Plugin.Misc.AdvancedProductFinder.Factories;
using Nop.Plugin.Misc.AdvancedProductFinder.Models;
using Nop.Web.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.AdvancedProductFinder.Controllers;

/// <summary>
/// Represents the public Advanced Product Finder controller
/// </summary>
public class AdvancedProductFinderController : BasePublicController
{
    #region Fields

    protected readonly AdvancedProductFinderSettings _settings;
    protected readonly IAdvancedProductFinderModelFactory _finderModelFactory;

    #endregion

    #region Ctor

    public AdvancedProductFinderController(
        AdvancedProductFinderSettings settings,
        IAdvancedProductFinderModelFactory finderModelFactory)
    {
        _settings = settings;
        _finderModelFactory = finderModelFactory;
    }

    #endregion

    #region Methods

    [HttpsRequirement]
    public virtual async Task<IActionResult> Index(FinderModel model)
    {
        if (!_settings.Enabled)
            return RedirectToRoute(NopRouteNames.General.HOMEPAGE);

        model ??= new FinderModel();
        model = await _finderModelFactory.PrepareFinderModelAsync(model);

        return View("~/Plugins/Misc.AdvancedProductFinder/Views/Finder.cshtml", model);
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public virtual async Task<IActionResult> GetProducts(FinderModel model)
    {
        if (!_settings.Enabled)
            return NotFound();

        model ??= new FinderModel();
        var productsModel = await _finderModelFactory.PrepareFinderProductsModelAsync(model);

        return PartialView("~/Plugins/Misc.AdvancedProductFinder/Views/_Products.cshtml", productsModel);
    }

    #endregion
}
