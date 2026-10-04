using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingCompany.Core.Entities;
using TradingCompany.DAL.Repositories;
using Xunit;

namespace TradingCompany.DAL.Tests;

public class OrderRepositoryTests : TestBase
{
    private readonly OrderRepository _orderRepo;
    private readonly UserRepository _userRepo;
    private readonly RoleRepository _roleRepo;

    public OrderRepositoryTests()
    {
        _orderRepo = new OrderRepository(Context);
        _userRepo = new UserRepository(Context);
        _roleRepo = new RoleRepository(Context);
    }

    [Fact]
    public async Task AddAsync_ShouldCreateOrderForCustomer()
    {
        var role = new Role { Name = "CustomerRole_" + Guid.NewGuid() };
        await _roleRepo.AddAsync(role);

        var user = new User { FullName = "Клієнт", Email = $"client_{Guid.NewGuid()}@test.com", RoleId = role.Id };
        await _userRepo.AddAsync(user);

        var order = new Order
        {
            OrderNumber = "ORD-" + Guid.NewGuid().ToString()[..6],
            CustomerId = user.Id,
            TotalAmount = 1500
        };

        await _orderRepo.AddAsync(order);

        var retrieved = await _orderRepo.GetByIdAsync(order.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(order.OrderNumber, retrieved.OrderNumber);
    }
}