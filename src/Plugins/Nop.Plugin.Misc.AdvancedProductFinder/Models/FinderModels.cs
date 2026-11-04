using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Misc.AdvancedProductFinder.Models;

/// <summary>
/// Represents the Advanced Product Finder page model
/// </summary>
public record FinderModel : BaseNopModel
{
    public string Keywords { get; set; }

    public int CategoryId { get; set; }

    public int ManufacturerId { get; set; }

    public string PriceFrom { get; set; }

    public string PriceTo { get; set; }

    public string SpecOptionIds { get; set; }

    public int PageNumber { get; set; } = 1;

    public int OrderBy { get; set; }

    public IList<SelectListItem> AvailableCategories { get; set; } = new List<SelectListItem>();

    public IList<SelectListItem> AvailableManufacturers { get; set; } = new List<SelectListItem>();

    public IList<SelectListItem> AvailableSortOptions { get; set; } = new List<SelectListItem>();

    public IList<FinderSpecAttributeModel> SpecificationAttributes { get; set; } = new List<FinderSpecAttributeModel>();

    public FinderProductsModel Products { get; set; } = new();

    public IList<ProductOverviewModel> RecentlyViewedProducts { get; set; } = new List<ProductOverviewModel>();

    public IList<ProductOverviewModel> CompleteTheSetProducts { get; set; } = new List<ProductOverviewModel>();

    public bool ShowRecentlyViewed { get; set; }

    public bool ShowCompleteTheSet { get; set; }

    public string GetProductsUrl { get; set; }
}

/// <summary>
/// Represents a specification attribute filter group
/// </summary>
public record FinderSpecAttributeModel : BaseNopEntityModel
{
    public string Name { get; set; }

    public bool HasColorSquares { get; set; }

    public IList<FinderSpecOptionModel> Options { get; set; } = new List<FinderSpecOptionModel>();
}

/// <summary>
/// Represents a specification attribute option chip
/// </summary>
public record FinderSpecOptionModel : BaseNopEntityModel
{
    public string Name { get; set; }

    public string ColorSquaresRgb { get; set; }

    public bool Selected { get; set; }
}

/// <summary>
/// Represents finder product results
/// </summary>
public record FinderProductsModel : BaseNopModel
{
    public IList<ProductOverviewModel> Products { get; set; } = new List<ProductOverviewModel>();

    public int TotalItems { get; set; }

    public int PageIndex { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalPages { get; set; }

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public string WarningMessage { get; set; }

    public string NoResultMessage { get; set; }
}

/// <summary>
/// Represents the homepage widget model
/// </summary>
public record FinderWidgetModel : BaseNopModel
{
    public string Title { get; set; }

    public string Description { get; set; }

    public string ButtonText { get; set; }

    public string FinderUrl { get; set; }
}
