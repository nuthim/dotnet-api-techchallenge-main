using FluentAssertions;
using ProductCatalog.Api.Domain.Product.Strategies;
using ProductCatalog.Tests.DataHelpers;
using Xunit;

namespace ProductCatalog.Tests;

public class SortStrategyTests
{
    [Fact]
    public void ShouldReturnEmptyListOfProducts()
    {
        var strategy = new SortByPriceLow();
        var result = strategy.Sort([], []);
        result.Should().BeEmpty();
    }

    [Fact]
    public void ShouldReturnSortedFromLowToHigh()
    {
        var strategy = new SortByPriceLow();
        var result = strategy.Sort(ListOfProduct.ANotSortedProductsFormLowToHigh, []);
        result.Should().Equal(ListOfProduct.SortedProductsFormLowToHigh);
    }

    [Fact]
    public void ShouldReturnSortedFromHighToLow()
    {
        var strategy = new SortByPriceHigh();
        var result = strategy.Sort(ListOfProduct.ANotSortedProductsFormLowToHigh, []);
        result.Should().Equal(ListOfProduct.SortedProductsFormHighToLow);
    }

    [Fact]
    public void ShouldReturnSortedAscending()
    {
        var strategy = new SortByNameAscending();
        var result = strategy.Sort(ListOfProduct.ANotSortedProductsFormLowToHigh, []);
        result.Should().Equal(ListOfProduct.SortedAscending);
    }

    [Fact]
    public void ShouldReturnSortedDescending()
    {
        var strategy = new SortByNameDescending();
        var result = strategy.Sort(ListOfProduct.ANotSortedProductsFormLowToHigh, []);
        result.Should().Equal(ListOfProduct.SortedDescending);
    }

    [Fact]
    public void ShouldReturnSortedRecommended()
    {
        var strategy = new SortByRecommended();
        var result = strategy.Sort(ListOfProduct.ANotSortedProductsFormLowToHigh, ShopperHistoryData.History);
        result.Should().Equal(ListOfProduct.SortedBasedOnRecommended);
    }
}
