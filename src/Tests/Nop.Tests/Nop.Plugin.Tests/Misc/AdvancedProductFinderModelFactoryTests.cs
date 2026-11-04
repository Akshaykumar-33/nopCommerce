using AwesomeAssertions;
using Moq;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Misc.AdvancedProductFinder;
using Nop.Plugin.Misc.AdvancedProductFinder.Factories;
using Nop.Plugin.Misc.AdvancedProductFinder.Models;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Routing;
using NUnit.Framework;

namespace Nop.Tests.Nop.Plugin.Tests.Misc;

[TestFixture]
public class AdvancedProductFinderModelFactoryTests
{
    private Mock<IProductService> _productService;
    private AdvancedProductFinderModelFactory _factory;

    private static ProductSortingEnum FirstOfferedSortOption =>
        Enum.GetValues<ProductSortingEnum>().First(value => value != ProductSortingEnum.Position);

    [SetUp]
    public void SetUp()
    {
        _productService = new Mock<IProductService>();
        _productService
            .Setup(service => service.SearchProductsAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<IList<int>>(),
                It.IsAny<IList<int>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<ProductType?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<decimal?>(),
                It.IsAny<decimal?>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<int>(),
                It.IsAny<IList<SpecificationAttributeOption>>(),
                It.IsAny<ProductSortingEnum>(),
                It.IsAny<bool>(),
                It.IsAny<bool?>()))
            .ReturnsAsync(new PagedList<Product>(new List<Product>(), 0, 1));

        var localizationService = new Mock<ILocalizationService>();
        localizationService
            .Setup(service => service.GetResourceAsync(It.IsAny<string>()))
            .ReturnsAsync((string key) => key);
        localizationService
            .Setup(service => service.GetLocalizedEnumAsync(It.IsAny<ProductSortingEnum>(), It.IsAny<int?>()))
            .ReturnsAsync((ProductSortingEnum value, int? _) => value.ToString());

        var storeContext = new Mock<IStoreContext>();
        storeContext.Setup(service => service.GetCurrentStoreAsync()).ReturnsAsync(new Store { Id = 1 });

        var workContext = new Mock<IWorkContext>();
        workContext.Setup(service => service.GetWorkingLanguageAsync()).ReturnsAsync(new Language { Id = 1 });

        var categoryService = new Mock<ICategoryService>();
        categoryService
            .Setup(service => service.GetAllCategoriesAsync(It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync(new List<Category>());

        var manufacturerService = new Mock<IManufacturerService>();
        manufacturerService
            .Setup(service => service.GetAllManufacturersAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<bool?>()))
            .ReturnsAsync(new PagedList<Manufacturer>(new List<Manufacturer>(), 0, 1));

        var specificationAttributeService = new Mock<ISpecificationAttributeService>();
        specificationAttributeService
            .Setup(service => service.GetSpecificationAttributesWithOptionsAsync())
            .ReturnsAsync(new List<SpecificationAttribute>());

        var settings = new AdvancedProductFinderSettings
        {
            ShowRecentlyViewed = false,
            ShowCompleteTheSet = false,
            ProductsPerPage = 8
        };

        _factory = new AdvancedProductFinderModelFactory(
            settings,
            new CatalogSettings(),
            Mock.Of<IAclService>(),
            categoryService.Object,
            Mock.Of<ICurrencyService>(),
            localizationService.Object,
            manufacturerService.Object,
            Mock.Of<INopUrlHelper>(),
            Mock.Of<IProductModelFactory>(),
            _productService.Object,
            Mock.Of<IRecentlyViewedProductsService>(),
            specificationAttributeService.Object,
            storeContext.Object,
            Mock.Of<IStoreMappingService>(),
            workContext.Object);
    }

    [Test]
    public async Task DefaultSortSelectionMatchesTheProductQuery()
    {
        var model = await _factory.PrepareFinderModelAsync(new FinderModel());
        var expected = FirstOfferedSortOption;

        model.AvailableSortOptions.Should().NotContain(option => option.Value == ((int)ProductSortingEnum.Position).ToString());
        var selected = model.AvailableSortOptions.Single(option => option.Selected);
        selected.Value.Should().Be(((int)expected).ToString());
        model.OrderBy.Should().Be((int)expected);
        SearchedOrderBy().Should().Be(expected);
    }

    [Test]
    public async Task ProductSearchWithoutSortOptionsUsesTheSameFallback()
    {
        var model = new FinderModel();

        await _factory.PrepareFinderProductsModelAsync(model);

        var expected = FirstOfferedSortOption;
        model.OrderBy.Should().Be((int)expected);
        SearchedOrderBy().Should().Be(expected);
    }

    [Test]
    public async Task ExplicitSortOptionIsKeptForTheDropdownAndTheQuery()
    {
        var model = await _factory.PrepareFinderModelAsync(new FinderModel
        {
            OrderBy = (int)ProductSortingEnum.PriceDesc
        });

        model.OrderBy.Should().Be((int)ProductSortingEnum.PriceDesc);
        model.AvailableSortOptions.Single(option => option.Selected).Value
            .Should().Be(((int)ProductSortingEnum.PriceDesc).ToString());
        SearchedOrderBy().Should().Be(ProductSortingEnum.PriceDesc);
    }

    [Test]
    public async Task UnknownSortOptionFallsBackWithTheDropdown()
    {
        var model = await _factory.PrepareFinderModelAsync(new FinderModel
        {
            OrderBy = 99
        });
        var expected = FirstOfferedSortOption;

        model.OrderBy.Should().Be((int)expected);
        model.AvailableSortOptions.Single(option => option.Selected).Value
            .Should().Be(((int)expected).ToString());
        SearchedOrderBy().Should().Be(expected);
    }

    private ProductSortingEnum SearchedOrderBy()
    {
        var invocation = _productService.Invocations
            .Single(call => call.Method.Name == nameof(IProductService.SearchProductsAsync));

        return invocation.Arguments.OfType<ProductSortingEnum>().Single();
    }
}
