using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingCompany.Core.Entities;
using TradingCompany.DAL.Repositories;
using Xunit;

namespace TradingCompany.DAL.Tests;

public class RoleRepositoryTests : TestBase
{
    private readonly RoleRepository _repository;

    public RoleRepositoryTests()
    {
        _repository = new RoleRepository(Context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddRoleToDatabase()
    {
        var role = new Role { Name = "Test_Manager_" + Guid.NewGuid() };

        await _repository.AddAsync(role);

        var retrieved = await _repository.GetByIdAsync(role.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(role.Name, retrieved.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveRole()
    {
        var role = new Role { Name = "Test_Role_Delete_" + Guid.NewGuid() };
        await _repository.AddAsync(role);

        await _repository.DeleteAsync(role.Id);

        var retrieved = await _repository.GetByIdAsync(role.Id);
        Assert.Null(retrieved);
    }
}
