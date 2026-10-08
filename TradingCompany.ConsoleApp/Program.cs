using Microsoft.EntityFrameworkCore;
using TradingCompany.Core.Entities;
using TradingCompany.DAL.Context;
using TradingCompany.DAL.Repositories;

namespace TradingCompany.ConsoleApp;

class Program
{
    private const string ConnectionString = "Server=DESKTOP-QPRTM95;Database=projectTradingCompany;Trusted_Connection=True;TrustServerCertificate=True;";

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var optionsBuilder = new DbContextOptionsBuilder<TradingDbContext>();
        optionsBuilder.UseSqlServer(ConnectionString);

        using var context = new TradingDbContext(optionsBuilder.Options);

        var roleRepo = new RoleRepository(context);
        var productRepo = new ProductRepository(context);

        var menu = new ConsoleMenu();

        menu.AddItem("1", "View all roles (Read)", () => ShowAllRoles(roleRepo));
        menu.AddItem("2", "Add a new role (Create)", () => AddRole(roleRepo));
        menu.AddItem("3", "Delete role by ID (Delete)", () => DeleteRole(roleRepo));
        menu.AddItem("4", "View all products (Read)", () => ShowAllProducts(productRepo));
        menu.AddItem("5", "Add a new product (Create)", () => AddProduct(productRepo));
        menu.AddItem("6", "Update product price (Update)", () => UpdateProductPrice(productRepo));

        await menu.DisplayAndExecuteAsync();
    }


    private static async Task ShowAllRoles(RoleRepository repo)
    {
        Console.WriteLine("ROLE LIST ");
        var roles = await repo.GetAllAsync();
        foreach (var r in roles)
        {
            Console.WriteLine($"ID: {r.Id} | Name: {r.Name}");
        }
    }

    private static async Task AddRole(RoleRepository repo)
    {
        Console.Write("Enter new role name: ");
        var name = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var role = new Role { Name = name };
            await repo.AddAsync(role);
            Console.WriteLine($"Role successfully created with ID: {role.Id}");
        }
    }

    private static async Task DeleteRole(RoleRepository repo)
    {
        Console.Write("Enter role ID to delete: ");
        if (int.TryParse(Console.ReadLine(), out int id))
        {
            await repo.DeleteAsync(id);
            Console.WriteLine($"Role with ID {id} deleted successfully.");
        }
    }


    private static async Task ShowAllProducts(ProductRepository repo)
    {
        Console.WriteLine(" PRODUCT LIST ");
        var products = await repo.GetAllAsync();
        foreach (var p in products)
        {
            Console.WriteLine($"ID: {p.Id} | SKU: {p.SKU} | Name: {p.Name} | Price: ${p.UnitPrice} | Weight: {p.WeightKg} kg");
        }
    }

    private static async Task AddProduct(ProductRepository repo)
    {
        Console.Write("Enter SKU: ");
        var sku = Console.ReadLine() ?? "SKU_000";

        Console.Write("Enter product name: ");
        var name = Console.ReadLine() ?? "New Product";

        Console.Write("Enter unit price: ");
        decimal.TryParse(Console.ReadLine(), out decimal price);

        var product = new Product
        {
            SKU = sku,
            Name = name,
            UnitPrice = price,
            WeightKg = 1.0m
        };

        await repo.AddAsync(product);
        Console.WriteLine($"Product '{product.Name}' added successfully with ID: {product.Id}");
    }

    private static async Task UpdateProductPrice(ProductRepository repo)
    {
        Console.Write("Enter product ID to update price: ");
        if (int.TryParse(Console.ReadLine(), out int id))
        {
            var product = await repo.GetByIdAsync(id);
            if (product != null)
            {
                Console.WriteLine($"Current price for '{product.Name}': ${product.UnitPrice}");
                Console.Write("Enter new price: ");
                if (decimal.TryParse(Console.ReadLine(), out decimal newPrice))
                {
                    product.UnitPrice = newPrice;
                    await repo.UpdateAsync(product);
                    Console.WriteLine("Price updated successfully");
                }
            }
            else
            {
                Console.WriteLine("Product with the specified ID was not found.");
            }
        }
    }
}