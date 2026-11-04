using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.AdvancedProductFinder.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.AdvancedProductFinder.Controllers;

/// <summary>
/// Represents the admin Advanced Product Finder controller
/// </summary>
[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class AdvancedProductFinderAdminController : BasePluginController
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly ISettingService _settingService;
    protected readonly IStoreContext _storeContext;

    #endregion

    #region Ctor

    public AdvancedProductFinderAdminController(
        ILocalizationService localizationService,
        INotificationService notificationService,
        ISettingService settingService,
        IStoreContext storeContext)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _settingService = settingService;
        _storeContext = storeContext;
    }

    #endregion

    #region Methods

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> Configure()
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<AdvancedProductFinderSettings>(storeScope);

        var model = new ConfigurationModel
        {
            Enabled = settings.Enabled,
            ShowHomepageWidget = settings.ShowHomepageWidget,
            ShowRecentlyViewed = settings.ShowRecentlyViewed,
            ShowCompleteTheSet = settings.ShowCompleteTheSet,
            ProductsPerPage = settings.ProductsPerPage,
            CompleteTheSetNumber = settings.CompleteTheSetNumber,
            WidgetZone = settings.WidgetZone,
            ActiveStoreScopeConfiguration = storeScope
        };

        if (storeScope > 0)
        {
            model.Enabled_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.Enabled, storeScope);
            model.ShowHomepageWidget_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ShowHomepageWidget, storeScope);
            model.ShowRecentlyViewed_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ShowRecentlyViewed, storeScope);
            model.ShowCompleteTheSet_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ShowCompleteTheSet, storeScope);
            model.ProductsPerPage_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ProductsPerPage, storeScope);
            model.CompleteTheSetNumber_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.CompleteTheSetNumber, storeScope);
            model.WidgetZone_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.WidgetZone, storeScope);
        }

        return View("~/Plugins/Misc.AdvancedProductFinder/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<AdvancedProductFinderSettings>(storeScope);

        settings.Enabled = model.Enabled;
        settings.ShowHomepageWidget = model.ShowHomepageWidget;
        settings.ShowRecentlyViewed = model.ShowRecentlyViewed;
        settings.ShowCompleteTheSet = model.ShowCompleteTheSet;
        settings.ProductsPerPage = model.ProductsPerPage > 0 ? model.ProductsPerPage : 12;
        settings.CompleteTheSetNumber = model.CompleteTheSetNumber > 0 ? model.CompleteTheSetNumber : 4;
        settings.WidgetZone = string.IsNullOrWhiteSpace(model.WidgetZone)
            ? "home_page_before_products"
            : model.WidgetZone.Trim();

        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.Enabled, model.Enabled_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ShowHomepageWidget, model.ShowHomepageWidget_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ShowRecentlyViewed, model.ShowRecentlyViewed_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ShowCompleteTheSet, model.ShowCompleteTheSet_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ProductsPerPage, model.ProductsPerPage_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.CompleteTheSetNumber, model.CompleteTheSetNumber_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.WidgetZone, model.WidgetZone_OverrideForStore, storeScope, false);

        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    #endregion
}
