using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TradingCompany.BL;
using TradingCompany.ConsoleApp.Mapping;
using TradingCompany.Core.DTOs;
using TradingCompany.DAL.Context;
using TradingCompany.DAL.Interfaces;
using TradingCompany.DAL.Repositories;

namespace TradingCompany.ConsoleApp;

class Program
{
    private const string ConnectionString = "Server=DESKTOP-QPRTM95;Database=projectTradingCompany;Trusted_Connection=True;TrustServerCertificate=True;";

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        var optionsBuilder = new DbContextOptionsBuilder<TradingDbContext>();
        optionsBuilder.UseSqlServer(ConnectionString);

        using var context = new TradingDbContext(optionsBuilder.Options);

        IUserRepository userRepo = new UserRepository(context);
        IOrderRepository orderRepo = new OrderRepository(context);

        IAuthService authService = new AuthService(userRepo, new PasswordHasher(), new UserSession());

        var menu = new ConsoleMenu();

        menu.AddItem("1", "Users: list all", () => ShowUsers(authService));
        menu.AddItem("2", "Users: create customer", () => CreateUser(authService, isDeliveryManager: false));
        menu.AddItem("3", "Users: create delivery manager", () => CreateUser(authService, isDeliveryManager: true));
        menu.AddItem("4", "Users: change password", () => ChangePassword(authService));
        menu.AddItem("5", "Users: delete", () => DeleteUser(authService));
        menu.AddItem("6", "Orders: list all", () => ShowOrders(orderRepo));
        menu.AddItem("7", "Orders: create", () => CreateOrder(orderRepo, userRepo));

        await menu.DisplayAndExecuteAsync();
    }

    private static async Task ShowUsers(IAuthService authService)
    {
        Console.WriteLine("USER LIST");
        foreach (var user in await authService.GetAllAsync())
            PrintUser(user);
    }

    private static async Task CreateUser(IAuthService authService, bool isDeliveryManager)
    {
        var dto = new CreateUserDto
        {
            Login = ReadString("Login: "),
            Password = ReadPassword("Password (min 6): "),
            FullName = ReadString("Full name: "),
            PhoneNumber = ReadString("Phone: "),
            Email = ReadString("Email: "),
            IsDeliveryManager = isDeliveryManager
        };

        try
        {
            var id = await authService.CreateUserAsync(dto);
            Console.WriteLine($"User created with ID: {id}");
        }
        catch (BusinessRuleException ex)
        {
            PrintErrors(ex);
        }
    }

    private static async Task ChangePassword(IAuthService authService)
    {
        var id = ReadInt("User ID: ");
        if (id is null) return;

        try
        {
            var changed = await authService.ChangePasswordAsync(id.Value, ReadPassword("New password (min 6): "));
            Console.WriteLine(changed ? "Password changed." : "User not found.");
        }
        catch (BusinessRuleException ex)
        {
            PrintErrors(ex);
        }
    }

    private static async Task DeleteUser(IAuthService authService)
    {
        var id = ReadInt("User ID to delete: ");
        if (id is null) return;

        try
        {
            var deleted = await authService.DeleteAsync(id.Value);
            Console.WriteLine(deleted ? $"User {id} deleted." : "User not found.");
        }
        catch (BusinessRuleException ex)
        {
            PrintErrors(ex);
        }
        catch (DbUpdateException)
        {
            Console.WriteLine("Cannot delete: the user has waybills or shipments.");
        }
    }

    private static async Task ShowOrders(IOrderRepository repo)
    {
        Console.WriteLine("ORDER LIST");
        var orders = (await repo.GetAllAsync()).Select(o => o.ToDto());
        foreach (var dto in orders)
            Console.WriteLine($"ID: {dto.Id} | No: {dto.OrderNumber} | Customer ID: {dto.CustomerId} | Date: {dto.OrderDate:dd.MM.yyyy HH:mm} | Total: {dto.TotalAmount:N2}");
    }

    private static async Task CreateOrder(IOrderRepository orderRepo, IUserRepository userRepo)
    {
        var dto = new CreateOrderDto
        {
            OrderNumber = ReadString("Order number: "),
            CustomerId = ReadInt("Customer ID: ") ?? 0,
            TotalAmount = ReadDecimal("Total amount: ") ?? -1
        };

        if (string.IsNullOrWhiteSpace(dto.OrderNumber) || dto.TotalAmount < 0)
        {
            Console.WriteLine("Order number is required and total must be >= 0.");
            return;
        }

        if (await userRepo.GetByIdAsync(dto.CustomerId) is null)
        {
            Console.WriteLine("Customer with the specified ID was not found.");
            return;
        }

        var entity = dto.ToEntity();
        try
        {
            await orderRepo.AddAsync(entity);
            Console.WriteLine($"Order {entity.OrderNumber} created with ID: {entity.Id}");
        }
        catch (DbUpdateException)
        {
            Console.WriteLine("Order with this number already exists.");
        }
    }

    private static void PrintUser(UserDto dto) =>
        Console.WriteLine($"ID: {dto.Id} | Login: {dto.Login} | {dto.FullName} | {dto.Email} | Role: {(dto.IsDeliveryManager ? "Delivery manager" : "Customer")}");

    private static void PrintErrors(BusinessRuleException ex)
    {
        foreach (var error in ex.Errors)
            Console.WriteLine($" - {error}");
    }

    private static string ReadString(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }

    private static string ReadPassword(string prompt)
    {
        Console.Write(prompt);
        var password = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
                break;

            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password.Length--;
                    Console.Write("\b \b");
                }
            }
            else if (!char.IsControl(key.KeyChar))
            {
                password.Append(key.KeyChar);
                Console.Write('*');
            }
        }

        Console.WriteLine();
        return password.ToString();
    }

    private static int? ReadInt(string prompt)
    {
        if (int.TryParse(ReadString(prompt), out var value))
            return value;

        Console.WriteLine("Invalid number.");
        return null;
    }

    private static decimal? ReadDecimal(string prompt)
    {
        var input = ReadString(prompt).Replace(',', '.');
        return decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }
}
