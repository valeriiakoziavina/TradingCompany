using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingCompany.Core.Entities;
using TradingCompany.DAL.Repositories;
using Xunit;

namespace TradingCompany.DAL.Tests;

public class OrderItemRepositoryTests : TestBase
{
    private readonly OrderItemRepository _itemRepo;

    public OrderItemRepositoryTests()
    {
        _itemRepo = new OrderItemRepository(Context);
    }

    [Fact]
    public async Task AddAsync_ShouldCreateOrderItem()
    {
        var role = new Role { Name = "Role_" + Guid.NewGuid() };
        var user = new User { FullName = "Тест", Email = $"e_{Guid.NewGuid()}@t.com", Role = role };
        var order = new Order { OrderNumber = "O_" + Guid.NewGuid().ToString()[..5], Customer = user, TotalAmount = 100 };
        var product = new Product { SKU = "S_" + Guid.NewGuid().ToString()[..5], Name = "P", UnitPrice = 50, WeightKg = 1 };

        await Context.Roles.AddAsync(role);
        await Context.Users.AddAsync(user);
        await Context.Orders.AddAsync(order);
        await Context.Products.AddAsync(product);
        await Context.SaveChangesAsync();  

        var item = new OrderItem { OrderId = order.Id, ProductId = product.Id, Quantity = 2, UnitPrice = 50 };
        await _itemRepo.AddAsync(item);

        var retrieved = await _itemRepo.GetByIdAsync(item.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(2, retrieved.Quantity);
    }
}
