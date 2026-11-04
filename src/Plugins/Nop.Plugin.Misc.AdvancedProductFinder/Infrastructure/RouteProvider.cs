using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace Nop.Plugin.Misc.AdvancedProductFinder.Infrastructure;

/// <summary>
/// Represents plugin route provider
/// </summary>
public class RouteProvider : BaseRouteProvider, IRouteProvider
{
    /// <summary>
    /// Register routes
    /// </summary>
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapControllerRoute(name: AdvancedProductFinderDefaults.ConfigurationRouteName,
            pattern: "Admin/AdvancedProductFinder/Configure",
            defaults: new { controller = "AdvancedProductFinderAdmin", action = "Configure", area = AreaNames.ADMIN });

        var lang = GetLanguageRoutePattern();

        endpointRouteBuilder.MapControllerRoute(name: AdvancedProductFinderDefaults.FinderRouteName,
            pattern: $"{lang}/advanced-product-finder",
            defaults: new { controller = "AdvancedProductFinder", action = "Index" });

        endpointRouteBuilder.MapControllerRoute(name: AdvancedProductFinderDefaults.GetProductsRouteName,
            pattern: $"{lang}/advanced-product-finder/products",
            defaults: new { controller = "AdvancedProductFinder", action = "GetProducts" });
    }

    /// <summary>
    /// Gets a priority of route provider
    /// </summary>
    public int Priority => 0;
}
