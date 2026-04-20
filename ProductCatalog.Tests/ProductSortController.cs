using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using ProductCatalog.Api.Domain.Product;
using ProductCatalog.Tests.DataHelpers;
using Xunit;

namespace ProductCatalog.Tests;

public class ProductSortController
{
    [Fact]
    public async Task SortEndpointIsConfiguredAndReturnsCorrectJsonResponse()
    {
        // Arrange
        var httpClient = new WebApplicationFactory<Program>().Server.CreateClient();

        // Act
        var httpResponseMessage = await httpClient.GetAsync("/products?sortBy=High");

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var readAsStringAsync = await httpResponseMessage.Content.ReadAsStringAsync();
        var products = JsonSerializer.Deserialize<Product[]>(readAsStringAsync, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        products.Should().Equal(ListOfProduct.SortedProductsFormHighToLow);
    }

    [Fact]
    public async Task SortEndpointIsConfiguredAndReturnsCorrectJsonResponseForRecommended()
    {
        // Arrange
        var httpClient = new WebApplicationFactory<Program>().Server.CreateClient();

        // Act
        var httpResponseMessage = await httpClient.GetAsync("/products?sortBy=Recommended");

        // Assert
        httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        var readAsStringAsync = await httpResponseMessage.Content.ReadAsStringAsync();
        var products = JsonSerializer.Deserialize<Product[]>(readAsStringAsync, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        products.Should().Equal(ListOfProduct.SortedBasedOnRecommended);
    }
}
