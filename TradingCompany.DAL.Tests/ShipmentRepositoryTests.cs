using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingCompany.Core.Entities;
using TradingCompany.DAL.Repositories;
using Xunit;

namespace TradingCompany.DAL.Tests;

public class ShipmentRepositoryTests : TestBase
{
    private readonly ShipmentRepository _shipmentRepo;

    public ShipmentRepositoryTests()
    {
        _shipmentRepo = new ShipmentRepository(Context);
    }

    [Fact]
    public async Task AddAsync_ShouldCreateShipment()
    {
        var role = new Role { Name = "Role_" + Guid.NewGuid() };
        var user = new User { FullName = "Менеджер", Email = $"m_{Guid.NewGuid()}@t.com", Role = role };
        var order = new Order { OrderNumber = "O_" + Guid.NewGuid().ToString()[..5], Customer = user, TotalAmount = 100 };
        var waybill = new Waybill { WaybillNumber = "WB_" + Guid.NewGuid().ToString()[..5], Order = order, Customer = user, City = "Київ", PaymentType = "Карта", DeliveryMethod = "Пошта" };
        var status = new ShipmentStatus { Name = "Сформовано_" + Guid.NewGuid() };

        await Context.Roles.AddAsync(role);
        await Context.Users.AddAsync(user);
        await Context.Orders.AddAsync(order);
        await Context.Waybills.AddAsync(waybill);
        await Context.ShipmentStatuses.AddAsync(status);
        await Context.SaveChangesAsync();  

        var shipment = new Shipment
        {
            WaybillId = waybill.Id,
            ManagerId = user.Id,
            StatusId = status.Id,
            DispatchedAt = DateTime.Now
        };

        await _shipmentRepo.AddAsync(shipment);

        var retrieved = await _shipmentRepo.GetByIdAsync(shipment.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(waybill.Id, retrieved.WaybillId);
    }
}