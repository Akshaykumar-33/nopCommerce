using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.AdvancedProductFinder.Models;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Misc.AdvancedProductFinder.Factories;

/// <summary>
/// Represents the Advanced Product Finder model factory
/// </summary>
public class AdvancedProductFinderModelFactory : IAdvancedProductFinderModelFactory
{
    #region Fields

    protected readonly AdvancedProductFinderSettings _settings;
    protected readonly CatalogSettings _catalogSettings;
    protected readonly IAclService _aclService;
    protected readonly ICategoryService _categoryService;
    protected readonly ICurrencyService _currencyService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IManufacturerService _manufacturerService;
    protected readonly INopUrlHelper _nopUrlHelper;
    protected readonly IProductModelFactory _productModelFactory;
    protected readonly IProductService _productService;
    protected readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
    protected readonly ISpecificationAttributeService _specificationAttributeService;
    protected readonly IStoreContext _storeContext;
    protected readonly IStoreMappingService _storeMappingService;
    protected readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public AdvancedProductFinderModelFactory(
        AdvancedProductFinderSettings settings,
        CatalogSettings catalogSettings,
        IAclService aclService,
        ICategoryService categoryService,
        ICurrencyService currencyService,
        ILocalizationService localizationService,
        IManufacturerService manufacturerService,
        INopUrlHelper nopUrlHelper,
        IProductModelFactory productModelFactory,
        IProductService productService,
        IRecentlyViewedProductsService recentlyViewedProductsService,
        ISpecificationAttributeService specificationAttributeService,
        IStoreContext storeContext,
        IStoreMappingService storeMappingService,
        IWorkContext workContext)
    {
        _settings = settings;
        _catalogSettings = catalogSettings;
        _aclService = aclService;
        _categoryService = categoryService;
        _currencyService = currencyService;
        _localizationService = localizationService;
        _manufacturerService = manufacturerService;
        _nopUrlHelper = nopUrlHelper;
        _productModelFactory = productModelFactory;
        _productService = productService;
        _recentlyViewedProductsService = recentlyViewedProductsService;
        _specificationAttributeService = specificationAttributeService;
        _storeContext = storeContext;
        _storeMappingService = storeMappingService;
        _workContext = workContext;
    }

    #endregion

    #region Utilities

    protected virtual async Task PrepareCategoryFilterAsync(FinderModel model)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var categories = await _categoryService.GetAllCategoriesAsync(storeId: store.Id);

        model.AvailableCategories.Add(new SelectListItem
        {
            Value = "0",
            Text = await _localizationService.GetResourceAsync("Common.All")
        });

        foreach (var category in categories)
        {
            var breadcrumb = await _categoryService.GetCategoryBreadCrumbAsync(category, categories);
            var name = string.Empty;

            for (var i = 0; i < breadcrumb.Count; i++)
            {
                name += await _localizationService.GetLocalizedAsync(breadcrumb[i], x => x.Name);
                if (i != breadcrumb.Count - 1)
                    name += " >> ";
            }

            model.AvailableCategories.Add(new SelectListItem
            {
                Value = category.Id.ToString(),
                Text = name,
                Selected = model.CategoryId == category.Id
            });
        }
    }

    protected virtual async Task PrepareManufacturerFilterAsync(FinderModel model)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var manufacturers = await _manufacturerService.GetAllManufacturersAsync(storeId: store.Id);

        model.AvailableManufacturers.Add(new SelectListItem
        {
            Value = "0",
            Text = await _localizationService.GetResourceAsync("Common.All")
        });

        foreach (var manufacturer in manufacturers)
        {
            model.AvailableManufacturers.Add(new SelectListItem
            {
                Value = manufacturer.Id.ToString(),
                Text = await _localizationService.GetLocalizedAsync(manufacturer, x => x.Name),
                Selected = model.ManufacturerId == manufacturer.Id
            });
        }
    }

    protected virtual async Task PrepareSortOptionsAsync(FinderModel model)
    {
        foreach (ProductSortingEnum enumValue in Enum.GetValues(typeof(ProductSortingEnum)))
        {
            if (enumValue == ProductSortingEnum.Position)
                continue;

            model.AvailableSortOptions.Add(new SelectListItem
            {
                Text = await _localizationService.GetLocalizedEnumAsync(enumValue),
                Value = ((int)enumValue).ToString(),
                Selected = model.OrderBy == (int)enumValue
            });
        }

        if (!model.AvailableSortOptions.Any(x => x.Selected) && model.AvailableSortOptions.Any())
            model.AvailableSortOptions.First().Selected = true;
    }

    protected virtual List<int> ParseSpecOptionIds(string specOptionIds)
    {
        if (string.IsNullOrWhiteSpace(specOptionIds))
            return [];

        return specOptionIds
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => int.TryParse(x, out var id) ? id : 0)
            .Where(id => id > 0)
            .Distinct()
            .ToList();
    }

    protected virtual async Task<IList<SpecificationAttributeOption>> GetAvailableSpecOptionsAsync(FinderModel model)
    {
        if (model.CategoryId > 0)
            return await _specificationAttributeService.GetFiltrableSpecificationAttributeOptionsByCategoryIdAsync(model.CategoryId);

        if (model.ManufacturerId > 0)
            return await _specificationAttributeService.GetFiltrableSpecificationAttributeOptionsByManufacturerIdAsync(model.ManufacturerId);

        var attributes = await _specificationAttributeService.GetSpecificationAttributesWithOptionsAsync();
        var options = new List<SpecificationAttributeOption>();

        foreach (var attribute in attributes)
        {
            var attributeOptions = await _specificationAttributeService
                .GetSpecificationAttributeOptionsBySpecificationAttributeAsync(attribute.Id);
            options.AddRange(attributeOptions);
        }

        return options;
    }

    protected virtual async Task PrepareSpecificationFiltersAsync(FinderModel model)
    {
        var selectedIds = ParseSpecOptionIds(model.SpecOptionIds);
        var availableOptions = await GetAvailableSpecOptionsAsync(model);
        var workingLanguage = await _workContext.GetWorkingLanguageAsync();

        foreach (var option in availableOptions)
        {
            var attributeModel = model.SpecificationAttributes.FirstOrDefault(x => x.Id == option.SpecificationAttributeId);
            if (attributeModel is null)
            {
                var attribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(option.SpecificationAttributeId);
                if (attribute is null)
                    continue;

                attributeModel = new FinderSpecAttributeModel
                {
                    Id = attribute.Id,
                    Name = await _localizationService.GetLocalizedAsync(attribute, x => x.Name, workingLanguage.Id)
                };
                model.SpecificationAttributes.Add(attributeModel);
            }

            attributeModel.Options.Add(new FinderSpecOptionModel
            {
                Id = option.Id,
                Name = await _localizationService.GetLocalizedAsync(option, x => x.Name, workingLanguage.Id),
                ColorSquaresRgb = option.ColorSquaresRgb,
                Selected = selectedIds.Contains(option.Id)
            });
        }

        foreach (var attributeModel in model.SpecificationAttributes)
            attributeModel.HasColorSquares = attributeModel.Options.Any(x => !string.IsNullOrEmpty(x.ColorSquaresRgb));
    }

    protected virtual async Task<IList<ProductOverviewModel>> PrepareRecentlyViewedAsync()
    {
        if (!_settings.ShowRecentlyViewed || !_catalogSettings.RecentlyViewedProductsEnabled)
            return [];

        var products = await (await _recentlyViewedProductsService
                .GetRecentlyViewedProductsAsync(_catalogSettings.RecentlyViewedProductsNumber))
            .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
            .Where(p => _productService.ProductIsAvailable(p))
            .ToListAsync();

        if (!products.Any())
            return [];

        return (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToList();
    }

    protected virtual async Task<IList<ProductOverviewModel>> PrepareCompleteTheSetAsync(IList<ProductOverviewModel> recentlyViewed, FinderProductsModel products)
    {
        if (!_settings.ShowCompleteTheSet)
            return [];

        var seedProductId = recentlyViewed.FirstOrDefault()?.Id
            ?? products.Products.FirstOrDefault()?.Id
            ?? 0;

        if (seedProductId <= 0)
            return [];

        var relatedIds = (await _productService.GetRelatedProductsByProductId1Async(seedProductId))
            .Select(x => x.ProductId2)
            .ToArray();

        IList<Product> relatedProducts;

        if (relatedIds.Length > 0)
        {
            relatedProducts = await (await _productService.GetProductsByIdsAsync(relatedIds))
                .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
                .Where(p => _productService.ProductIsAvailable(p) && p.VisibleIndividually)
                .Take(_settings.CompleteTheSetNumber)
                .ToListAsync();
        }
        else
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var categories = await _categoryService.GetProductCategoriesByProductIdAsync(seedProductId);
            var categoryId = categories.FirstOrDefault()?.CategoryId ?? 0;
            if (categoryId <= 0)
                return [];

            var sameCategory = await _productService.SearchProductsAsync(
                pageIndex: 0,
                pageSize: _settings.CompleteTheSetNumber + 1,
                categoryIds: new List<int> { categoryId },
                storeId: store.Id,
                visibleIndividuallyOnly: true,
                orderBy: ProductSortingEnum.CreatedOn);

            relatedProducts = sameCategory.Where(p => p.Id != seedProductId).Take(_settings.CompleteTheSetNumber).ToList();
        }

        if (!relatedProducts.Any())
            return [];

        return (await _productModelFactory.PrepareProductOverviewModelsAsync(relatedProducts)).ToList();
    }

    /// <summary>
    /// Parses a price posted by an input type="number".
    /// The control's value is always a period-decimal number, even when the current culture uses a comma.
    /// </summary>
    /// <param name="rawPrice">Posted price</param>
    /// <param name="price">Parsed working-currency amount</param>
    /// <returns>True when <paramref name="rawPrice"/> is a valid number</returns>
    protected virtual bool TryParsePriceInput(string rawPrice, out decimal price)
    {
        price = decimal.Zero;

        if (string.IsNullOrWhiteSpace(rawPrice))
            return false;

        return decimal.TryParse(
            rawPrice.Trim(),
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
            CultureInfo.InvariantCulture,
            out price);
    }

    /// <summary>
    /// Rewrites a posted price into the period-decimal form an input type="number" can display
    /// </summary>
    /// <param name="rawPrice">Posted price</param>
    /// <returns>Invariant price text, or null when the value is empty or not a number</returns>
    protected virtual string NormalizePriceInput(string rawPrice)
    {
        if (!TryParsePriceInput(rawPrice, out var price))
            return null;

        return price.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts positive working-currency price inputs to the primary store currency stored on <see cref="Product.Price"/>
    /// </summary>
    /// <param name="priceFrom">Minimum price entered by the customer</param>
    /// <param name="priceTo">Maximum price entered by the customer</param>
    /// <returns>Primary-store-currency bounds. A bound is null when that input is empty or not a positive price</returns>
    protected virtual async Task<(decimal? Min, decimal? Max)> GetPriceBoundsInPrimaryStoreCurrencyAsync(string priceFrom, string priceTo)
    {
        var hasMin = TryParsePriceInput(priceFrom, out var from) && from > decimal.Zero;
        var hasMax = TryParsePriceInput(priceTo, out var to) && to > decimal.Zero;

        if (!hasMin && !hasMax)
            return (null, null);

        var workingCurrency = await _workContext.GetWorkingCurrencyAsync();

        var min = hasMin
            ? await _currencyService.ConvertToPrimaryStoreCurrencyAsync(from, workingCurrency)
            : (decimal?)null;

        var max = hasMax
            ? await _currencyService.ConvertToPrimaryStoreCurrencyAsync(to, workingCurrency)
            : (decimal?)null;

        return (min, max);
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public virtual async Task<FinderModel> PrepareFinderModelAsync(FinderModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        model.ShowRecentlyViewed = _settings.ShowRecentlyViewed;
        model.ShowCompleteTheSet = _settings.ShowCompleteTheSet;
        model.GetProductsUrl = _nopUrlHelper.RouteUrl(AdvancedProductFinderDefaults.GetProductsRouteName);

        await PrepareCategoryFilterAsync(model);
        await PrepareManufacturerFilterAsync(model);
        await PrepareSortOptionsAsync(model);
        await PrepareSpecificationFiltersAsync(model);

        var workingCurrency = await _workContext.GetWorkingCurrencyAsync();
        model.WorkingCurrencyCode = workingCurrency?.CurrencyCode;
        model.PriceFrom = NormalizePriceInput(model.PriceFrom);
        model.PriceTo = NormalizePriceInput(model.PriceTo);

        model.Products = await PrepareFinderProductsModelAsync(model);
        model.RecentlyViewedProducts = await PrepareRecentlyViewedAsync();
        model.CompleteTheSetProducts = await PrepareCompleteTheSetAsync(model.RecentlyViewedProducts, model.Products);

        return model;
    }

    /// <inheritdoc />
    public virtual async Task<FinderProductsModel> PrepareFinderProductsModelAsync(FinderModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var pageSize = Math.Max(1, _settings.ProductsPerPage);
        var pageIndex = Math.Max(0, model.PageNumber - 1);
        var store = await _storeContext.GetCurrentStoreAsync();
        var workingLanguage = await _workContext.GetWorkingLanguageAsync();

        var categoryIds = new List<int>();
        if (model.CategoryId > 0)
        {
            categoryIds.Add(model.CategoryId);
            categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(model.CategoryId, store.Id));
        }

        var manufacturerIds = model.ManufacturerId > 0
            ? new List<int> { model.ManufacturerId }
            : null;

        var (priceMin, priceMax) = await GetPriceBoundsInPrimaryStoreCurrencyAsync(model.PriceFrom, model.PriceTo);

        var selectedSpecIds = ParseSpecOptionIds(model.SpecOptionIds);
        IList<SpecificationAttributeOption> filteredSpecOptions = null;
        if (selectedSpecIds.Any())
            filteredSpecOptions = await _specificationAttributeService.GetSpecificationAttributeOptionsByIdsAsync(selectedSpecIds.ToArray());

        var orderBy = Enum.IsDefined(typeof(ProductSortingEnum), model.OrderBy)
            ? (ProductSortingEnum)model.OrderBy
            : ProductSortingEnum.Position;

        var products = await _productService.SearchProductsAsync(
            pageIndex: pageIndex,
            pageSize: pageSize,
            categoryIds: categoryIds,
            manufacturerIds: manufacturerIds,
            storeId: store.Id,
            visibleIndividuallyOnly: true,
            priceMin: priceMin,
            priceMax: priceMax,
            keywords: model.Keywords?.Trim(),
            searchDescriptions: true,
            searchProductTags: true,
            languageId: workingLanguage.Id,
            filteredSpecOptions: filteredSpecOptions,
            orderBy: orderBy);

        var result = new FinderProductsModel
        {
            TotalItems = products.TotalCount,
            PageIndex = products.PageIndex,
            PageNumber = products.PageIndex + 1,
            PageSize = products.PageSize,
            TotalPages = products.TotalPages
        };

        if (!products.Any())
        {
            result.NoResultMessage = await _localizationService.GetResourceAsync("Plugins.Misc.AdvancedProductFinder.NoResults");
            return result;
        }

        result.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToList();
        return result;
    }

    /// <inheritdoc />
    public virtual async Task<FinderWidgetModel> PrepareWidgetModelAsync()
    {
        return new FinderWidgetModel
        {
            Title = await _localizationService.GetResourceAsync("Plugins.Misc.AdvancedProductFinder.Widget.Title"),
            Description = await _localizationService.GetResourceAsync("Plugins.Misc.AdvancedProductFinder.Widget.Description"),
            ButtonText = await _localizationService.GetResourceAsync("Plugins.Misc.AdvancedProductFinder.Widget.Button"),
            FinderUrl = _nopUrlHelper.RouteUrl(AdvancedProductFinderDefaults.FinderRouteName)
        };
    }

    #endregion
}
