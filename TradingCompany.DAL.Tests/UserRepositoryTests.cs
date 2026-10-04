using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingCompany.Core.Entities;
using TradingCompany.DAL.Repositories;
using Xunit;

namespace TradingCompany.DAL.Tests;

public class UserRepositoryTests : TestBase
{
    private readonly UserRepository _userRepository;
    private readonly RoleRepository _roleRepository;

    public UserRepositoryTests()
    {
        _userRepository = new UserRepository(Context);
        _roleRepository = new RoleRepository(Context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddUserWithRole()
    {
        var role = new Role { Name = "TestRole_" + Guid.NewGuid() };
        await _roleRepository.AddAsync(role);

        var user = new User
        {
            FullName = "Іван Петренко",
            Email = $"ivan_{Guid.NewGuid()}@test.com",
            PhoneNumber = "+380971234567",
            RoleId = role.Id
        };

        await _userRepository.AddAsync(user);

        var retrieved = await _userRepository.GetByIdAsync(user.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(user.Email, retrieved.Email);
    }
}