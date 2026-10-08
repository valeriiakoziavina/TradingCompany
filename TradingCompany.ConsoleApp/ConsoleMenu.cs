namespace TradingCompany.ConsoleApp;

public class MenuItem
{
    public string Title { get; set; } = string.Empty;
    public Func<Task> Action { get; set; } = () => Task.CompletedTask;
}

public class ConsoleMenu
{
    private readonly Dictionary<string, MenuItem> _items = new();

    public void AddItem(string key, string title, Func<Task> action)
    {
        _items[key] = new MenuItem { Title = title, Action = action };
    }

    public async Task DisplayAndExecuteAsync()
    {
        bool exit = false;

        while (!exit)
        {
            Console.Clear();
            Console.WriteLine("        TRADING COMPANY MANAGEMENT SYSTEM         ");

            foreach (var item in _items)
            {
                Console.WriteLine($"{item.Key}. {item.Value.Title}");
            }
            Console.WriteLine("0. Exit application");
            Console.Write("Select an option: ");

            var input = Console.ReadLine();
            Console.WriteLine();

            if (input == "0")
            {
                exit = true;
                Console.WriteLine("Exiting application");
            }
            else if (input != null && _items.TryGetValue(input, out var menuItem))
            {
                await menuItem.Action();
                Console.WriteLine("\nPress Enter to return to the menu");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Invalid option. Press Enter to continue");
                Console.ReadLine();
            }
        }
    }
}