using System.Text;

namespace StoreSimulation;

// Товар у магазині
class Product
{
    public string Name { get; }
    public int Price { get; }
    public int Quantity { get; set; }

    public Product(string name, int price, int quantity)
    {
        Name = name;
        Price = price;
        Quantity = quantity;
    }
}

// Покупець
class Customer
{
    public int CardNumber { get; }
    public int Money { get; private set; }
    public int ItemsBought { get; private set; }
    public int MoneySpent { get; private set; }

    public Customer(int cardNumber, int money)
    {
        CardNumber = cardNumber;
        Money = money;
    }

    // Покупець може купити товар, якщо він є в наявності і вистачає грошей
    public bool CanBuy(Product product) => product.Quantity > 0 && Money >= product.Price;

    public void Pay(Product product)
    {
        Money -= product.Price;
        MoneySpent += product.Price;
        ItemsBought++;
    }
}

// Магазин
class Store
{
    public List<Product> Products { get; }
    public int Profit { get; private set; } = 0;

    public Store(List<Product> products)
    {
        Products = products;
    }

    // Товари, які цей покупець може собі дозволити
    public List<Product> GetAvailableFor(Customer customer) =>
        Products.Where(customer.CanBuy).ToList();

    // Продаж: кількість -1, гроші покупця -ціна, прибуток +ціна
    public void Sell(Customer customer, Product product)
    {
        product.Quantity--;
        customer.Pay(product);
        Profit += product.Price;
    }

    public bool HasAnyProducts() => Products.Any(p => p.Quantity > 0);
}

// Симуляція
class Simulation
{
    private readonly Store _store;
    private readonly List<Customer> _customers;
    private readonly Random _rnd = new Random();

    public Simulation(Store store, List<Customer> customers)
    {
        _store = store;
        _customers = customers;
    }

    public void Run()
    {
        bool someoneBought = true;

        // Раунди: у кожному раунді кожен покупець робить одну покупку (якщо може).
        // Закінчуємо, коли ніхто більше нічого не може купити
        // (у всіх закінчилися гроші, або товари закінчилися).
        while (someoneBought && _store.HasAnyProducts())
        {
            someoneBought = false;

            foreach (var customer in _customers)
            {
                var available = _store.GetAvailableFor(customer);
                if (available.Count == 0)
                    continue;

                var product = available[_rnd.Next(available.Count)];
                _store.Sell(customer, product);
                someoneBought = true;
            }
        }
    }

    public void PrintResults()
    {
        Console.WriteLine();
        Console.WriteLine("=== Результати ===");
        Console.WriteLine($"Прибуток магазину: {_store.Profit}");

        var mostItems = _customers.MaxBy(c => c.ItemsBought)!;
        var mostMoney = _customers.MaxBy(c => c.MoneySpent)!;

        Console.WriteLine($"Найбільше товарів купив: картка №{mostItems.CardNumber} ({mostItems.ItemsBought} шт.)");
        Console.WriteLine($"Найбільше грошей витратив: картка №{mostMoney.CardNumber} ({mostMoney.MoneySpent} грн)");
    }
}

class Program
{
    static List<Product> CreateProducts(Random rnd) => new()
    {
        new Product("Хліб",       25,  rnd.Next(20, 60)),
        new Product("Молоко",     40,  rnd.Next(20, 60)),
        new Product("Яйця",       60,  rnd.Next(20, 60)),
        new Product("Сир",        180, rnd.Next(10, 40)),
        new Product("М'ясо",      250, rnd.Next(10, 40)),
        new Product("Кава",       320, rnd.Next(5, 30)),
        new Product("Шоколад",    70,  rnd.Next(20, 60)),
        new Product("Риба",       280, rnd.Next(5, 30)),
        new Product("Фрукти",     90,  rnd.Next(20, 60)),
        new Product("Овочі",      55,  rnd.Next(20, 60)),
    };

    static List<Customer> CreateCustomers(int count, Random rnd)
    {
        var customers = new List<Customer>();
        var usedCards = new HashSet<int>(); // щоб номери карток не повторювалися

        while (customers.Count < count)
        {
            int card = rnd.Next(1, 1001);      // 1..1000
            if (!usedCards.Add(card))
                continue;

            int money = rnd.Next(1000, 10_001); // 1000..10000
            customers.Add(new Customer(card, money));
        }
        return customers;
    }

    static int ReadCustomerCount()
    {
        while (true)
        {
            Console.Write("Введіть кількість покупців (1..1000): ");
            if (int.TryParse(Console.ReadLine(), out int n) && n >= 1 && n <= 1000)
                return n;
            Console.WriteLine("Некоректне значення, спробуйте ще раз.");
        }
    }

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        var rnd = new Random();

        int count = ReadCustomerCount();

        var store = new Store(CreateProducts(rnd));
        var customers = CreateCustomers(count, rnd);

        var simulation = new Simulation(store, customers);
        simulation.Run();
        simulation.PrintResults();
    }
}
