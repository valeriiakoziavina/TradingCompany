using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingCompany.DAL.Context;


namespace TradingCompany.DAL.Tests;

public abstract class TestBase : IAsyncLifetime
{
    protected readonly TradingDbContext Context;
    private IDbContextTransaction? _transaction;

    private const string ConnectionString = "Server=DESKTOP-QPRTM95;Database=projectTradingCompany_Tests;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

    private static bool _databaseInitialized = false;
    private static readonly object _dbLock = new object();

    protected TestBase()
    {
        var options = new DbContextOptionsBuilder<TradingDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        Context = new TradingDbContext(options);

        lock (_dbLock)
        {
            if (!_databaseInitialized)
            {
                Context.Database.EnsureDeleted(); 
                Context.Database.EnsureCreated(); 
                _databaseInitialized = true;
            }
        }
    }

    public async Task InitializeAsync()
    {
        _transaction = await Context.Database.BeginTransactionAsync();
    }

    public async Task DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
        }

        await Context.DisposeAsync();
    }
}