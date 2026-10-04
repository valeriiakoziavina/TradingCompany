using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingCompany.Core.Entities;
using TradingCompany.DAL.Repositories;
using Xunit;

namespace TradingCompany.DAL.Tests;

public class ShipmentStatusRepositoryTests : TestBase
{
    private readonly ShipmentStatusRepository _repo;

    public ShipmentStatusRepositoryTests()
    {
        _repo = new ShipmentStatusRepository(Context);
    }

    [Fact]
    public async Task AddAsync_ShouldCreateStatus()
    {
        var status = new ShipmentStatus { Name = "В дорозі_" + Guid.NewGuid() };
        await _repo.AddAsync(status);

        var retrieved = await _repo.GetByIdAsync(status.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(status.Name, retrieved.Name);
    }
}