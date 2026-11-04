using System.Globalization;
using AwesomeAssertions;
using Moq;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Misc.AdvancedProductFinder;
using Nop.Plugin.Misc.AdvancedProductFinder.Factories;
using Nop.Plugin.Misc.AdvancedProductFinder.Models;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Models.Catalog;
using NUnit.Framework;

namespace Nop.Tests.Nop.Web.Tests.Public.Factories;

[TestFixture]
public class AdvancedProductFinderModelFactoryTests
{
    private CultureInfo _originalCulture;
    private Currency _euro;
    private Mock<ICurrencyService> _currencyService;
    private Mock<IProductService> _productService;
    private Mock<IWorkContext> _workContext;
    private AdvancedProductFinderModelFactory _factory;

    [SetUp]
    public void SetUp()
    {
        _originalCulture = CultureInfo.CurrentCulture;
        var commaCulture = new CultureInfo("de-DE");
        CultureInfo.CurrentCulture = commaCulture;
        CultureInfo.CurrentUICulture = commaCulture;

        _euro = new Currency
        {
            Id = 2,
            Name = "Euro",
            CurrencyCode = "EUR",
            Rate = 0.86m
        };

        _currencyService = new Mock<ICurrencyService>();
        _currencyService
            .Setup(service => service.ConvertToPrimaryStoreCurrencyAsync(It.IsAny<decimal>(), _euro))
            .ReturnsAsync((decimal amount, Currency currency) => amount / currency.Rate);

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
            .ReturnsAsync(new PagedList<Product>(new List<Product>(), 0, 1, 0));

        var localizationService = new Mock<ILocalizationService>();
        localizationService
            .Setup(service => service.GetResourceAsync(It.IsAny<string>()))
            .ReturnsAsync("none");

        _workContext = new Mock<IWorkContext>();
        _workContext.Setup(context => context.GetWorkingCurrencyAsync()).ReturnsAsync(_euro);
        _workContext.Setup(context => context.GetWorkingLanguageAsync()).ReturnsAsync(new Language { Id = 1 });

        var storeContext = new Mock<IStoreContext>();
        storeContext.Setup(context => context.GetCurrentStoreAsync()).ReturnsAsync(new Store { Id = 1 });

        _factory = new AdvancedProductFinderModelFactory(
            new AdvancedProductFinderSettings { ProductsPerPage = 8 },
            new CatalogSettings(),
            null,
            null,
            _currencyService.Object,
            localizationService.Object,
            null,
            new Mock<INopUrlHelper>().Object,
            null,
            _productService.Object,
            null,
            null,
            storeContext.Object,
            null,
            _workContext.Object);
    }

    [TearDown]
    public void TearDown()
    {
        CultureInfo.CurrentCulture = _originalCulture;
        CultureInfo.CurrentUICulture = _originalCulture;
    }

    [Test]
    public async Task PrepareFinderProductsModelAsync_parses_period_decimals_and_converts_working_currency()
    {
        decimal.TryParse("10.50", out var misreadFrom).Should().BeTrue();
        misreadFrom.Should().Be(1050m);
        decimal.TryParse("8.686", out var misreadTo).Should().BeTrue();
        misreadTo.Should().Be(8686m);

        await _factory.PrepareFinderProductsModelAsync(new FinderModel
        {
            PriceFrom = "10.50",
            PriceTo = "8.686"
        });

        var (priceMin, priceMax) = CapturedPriceBounds();

        priceMin.Should().Be(10.50m / _euro.Rate);
        priceMax.Should().Be(8.686m / _euro.Rate);
        _currencyService.Verify(service => service.ConvertToPrimaryStoreCurrencyAsync(10.50m, _euro), Times.Once);
        _currencyService.Verify(service => service.ConvertToPrimaryStoreCurrencyAsync(8.686m, _euro), Times.Once);
        _currencyService.Verify(service => service.ConvertToPrimaryStoreCurrencyAsync(1050m, It.IsAny<Currency>()), Times.Never);
        _currencyService.Verify(service => service.ConvertToPrimaryStoreCurrencyAsync(8686m, It.IsAny<Currency>()), Times.Never);
    }

    [Test]
    public async Task PrepareFinderProductsModelAsync_ignores_blank_zero_and_comma_prices()
    {
        await _factory.PrepareFinderProductsModelAsync(new FinderModel
        {
            PriceFrom = "10,50",
            PriceTo = "0"
        });

        var (priceMin, priceMax) = CapturedPriceBounds();

        priceMin.Should().BeNull();
        priceMax.Should().BeNull();
        _currencyService.Verify(service => service.ConvertToPrimaryStoreCurrencyAsync(It.IsAny<decimal>(), It.IsAny<Currency>()), Times.Never);
    }

    [Test]
    public async Task PrepareFinderModelAsync_redisplays_period_decimal_and_working_currency()
    {
        var factory = new PriceInputFactory(
            new AdvancedProductFinderSettings(),
            new CatalogSettings(),
            _currencyService.Object,
            new Mock<INopUrlHelper>().Object,
            _workContext.Object);

        var model = await factory.PrepareFinderModelAsync(new FinderModel
        {
            PriceFrom = " 10.50 ",
            PriceTo = "10,50"
        });

        model.PriceFrom.Should().Be("10.50");
        model.PriceTo.Should().BeNull();
        model.WorkingCurrencyCode.Should().Be("EUR");
    }

    private (decimal? Min, decimal? Max) CapturedPriceBounds()
    {
        var invocation = _productService.Invocations.Single(call => call.Method.Name == nameof(IProductService.SearchProductsAsync));

        // SearchProductsAsync argument order places priceMin at index 10 and priceMax at index 11.
        return ((decimal?)invocation.Arguments[10], (decimal?)invocation.Arguments[11]);
    }

    /// <summary>
    /// Skips catalog lookups so the test can call the real finder-page preparation
    /// </summary>
    private sealed class PriceInputFactory : AdvancedProductFinderModelFactory
    {
        public PriceInputFactory(
            AdvancedProductFinderSettings settings,
            CatalogSettings catalogSettings,
            ICurrencyService currencyService,
            INopUrlHelper nopUrlHelper,
            IWorkContext workContext)
            : base(settings, catalogSettings, null, null, currencyService, null, null, nopUrlHelper, null, null, null, null, null, null, workContext)
        {
        }

        protected override Task PrepareCategoryFilterAsync(FinderModel model)
        {
            return Task.CompletedTask;
        }

        protected override Task PrepareManufacturerFilterAsync(FinderModel model)
        {
            return Task.CompletedTask;
        }

        protected override Task PrepareSortOptionsAsync(FinderModel model)
        {
            return Task.CompletedTask;
        }

        protected override Task PrepareSpecificationFiltersAsync(FinderModel model)
        {
            return Task.CompletedTask;
        }

        public override Task<FinderProductsModel> PrepareFinderProductsModelAsync(FinderModel model)
        {
            return Task.FromResult(new FinderProductsModel());
        }

        protected override Task<IList<ProductOverviewModel>> PrepareRecentlyViewedAsync()
        {
            return Task.FromResult<IList<ProductOverviewModel>>(new List<ProductOverviewModel>());
        }

        protected override Task<IList<ProductOverviewModel>> PrepareCompleteTheSetAsync(IList<ProductOverviewModel> recentlyViewed, FinderProductsModel products)
        {
            return Task.FromResult<IList<ProductOverviewModel>>(new List<ProductOverviewModel>());
        }
    }
}
