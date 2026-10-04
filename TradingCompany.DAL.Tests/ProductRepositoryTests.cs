using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingCompany.Core.Entities;
using TradingCompany.DAL.Repositories;
using Xunit;

namespace TradingCompany.DAL.Tests;

public class ProductRepositoryTests : TestBase
{
    private readonly ProductRepository _repository;

    public ProductRepositoryTests()
    {
        _repository = new ProductRepository(Context);
    }

    [Fact]
    public async Task AddAndGetAll_ShouldReturnCreatedProduct()
    {
        var product = new Product
        {
            SKU = "SKU_" + Guid.NewGuid().ToString()[..8],
            Name = "Ноутбук Тестовий",
            UnitPrice = 25000,
            WeightKg = 2.1m
        };

        await _repository.AddAsync(product);

        var products = await _repository.GetAllAsync();
        Assert.Contains(products, p => p.SKU == product.SKU);
    }
}