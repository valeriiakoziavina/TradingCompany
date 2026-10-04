using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingCompany.Core.Entities;
using TradingCompany.DAL.Repositories;
using Xunit;

namespace TradingCompany.DAL.Tests;

public class WaybillRepositoryTests : TestBase
{
    private readonly WaybillRepository _waybillRepo;

    public WaybillRepositoryTests()
    {
        _waybillRepo = new WaybillRepository(Context);
    }

    [Fact]
    public async Task GetFilteredAsync_ShouldReturnMatchingWaybills()
    {
        var role = new Role { Name = "Role_" + Guid.NewGuid() };
        var user = new User { FullName = "Тест", Email = $"e_{Guid.NewGuid()}@t.com", Role = role };
        var order = new Order { OrderNumber = "O_" + Guid.NewGuid().ToString()[..5], Customer = user, TotalAmount = 100 };

        var waybill = new Waybill
        {
            WaybillNumber = "WB_" + Guid.NewGuid().ToString()[..5],
            Order = order,
            Customer = user,
            City = "Львів",
            PaymentType = "Безготівковий",
            DeliveryMethod = "Кур'єр"
        };

        await _waybillRepo.AddAsync(waybill);

        var filtered = await _waybillRepo.GetFilteredAsync(null, null, "Львів", "Безготівковий");
        Assert.Contains(filtered, w => w.WaybillNumber == waybill.WaybillNumber);
    }
}