using BlazingPizza.Shared;

namespace BlazingPizza.Tests;

public class OrderTests
{
    // ---------- Helpers ----------
    private static List<PizzaTopping> Toppings(params decimal[] prices)
    {
        var list = new List<PizzaTopping>(prices.Length);
        for (int i = 0; i < prices.Length; i++)
        {
            list.Add(new PizzaTopping
            {
                Topping = new Topping { Name = $"T{i + 1}", Price = prices[i] }
            });
        }
        return list;
    }

    private static Pizza P(int size, decimal basePrice, params decimal[] toppingPrices)
    {
        return new Pizza
        {
            Size = size,
            Special = new PizzaSpecial { Name = "Spec", BasePrice = basePrice },
            Toppings = Toppings(toppingPrices)
        };
    }

    private static decimal ExpectedPizzaTotal(Pizza p)
    {
        var basePart = ((decimal)p.Size / Pizza.DefaultSize) * p.Special!.BasePrice;
        var tops = p.Toppings.Sum(t => t.Topping!.Price);
        return basePart + tops;
    }

    private static (Order order, decimal expectedTotal, string expectedFormatted) MakeOrder(List<Pizza> pizzas)
    {
        var order = new Order { Pizzas = pizzas };
        var expected = pizzas.Sum(ExpectedPizzaTotal);
        var fmt = expected.ToString("0.00");
        return (order, expected, fmt);
    }

    // ---------- Base-Choice tests ----------
    [Test]
    public void EmptyOrder_TotalIsZero()
    {
        var (order, expected, fmt) = MakeOrder(new List<Pizza>());

        var total = order.GetTotalPrice();
        var formatted = order.GetFormattedTotalPrice();

        TestContext.WriteLine($"Empty → Expected={expected:0.00}, Actual={total:0.00}");
        Assert.That(total, Is.EqualTo(0m));
        Assert.That(formatted, Is.EqualTo("0.00"));
    }

    [Test]
    public void SinglePizza_OrderTotalEqualsPizzaTotal()
    {
        // Base choice: Single
        var pizza = P(size: 12, basePrice: 10.00m, toppingPrices: new[] { 1.00m });
        var (order, expected, fmt) = MakeOrder(new List<Pizza> { pizza });

        var total = order.GetTotalPrice();
        var formatted = order.GetFormattedTotalPrice();

        TestContext.WriteLine($"Single → PizzaTotal={ExpectedPizzaTotal(pizza):0.00}, OrderTotal={total:0.00}");
        Assert.That(total, Is.EqualTo(expected));
        Assert.That(formatted, Is.EqualTo(fmt));
    }

    [Test]
    public void Multiple_Uniform_AllPizzasIdentical_SumsCorrectly()
    {
        // Multiple + Uniform
        var p1 = P(size: 12, basePrice: 10.00m, toppingPrices: new[] { 1.00m });
        var p2 = P(size: 12, basePrice: 10.00m, toppingPrices: new[] { 1.00m });
        var p3 = P(size: 12, basePrice: 10.00m, toppingPrices: new[] { 1.00m });
        var p4 = P(size: 12, basePrice: 10.00m, toppingPrices: new[] { 1.00m });

        var (order, expected, fmt) = MakeOrder(new List<Pizza> { p1, p2, p3, p4 });

        var total = order.GetTotalPrice();
        var formatted = order.GetFormattedTotalPrice();

        TestContext.WriteLine($"Multiple+Uniform → Each={ExpectedPizzaTotal(p1):0.00}, Count=4, Total={total:0.00}");
        Assert.That(total, Is.EqualTo(expected));
        Assert.That(formatted, Is.EqualTo(fmt));
    }

    [Test]
    public void Multiple_Varied_MixesSizesAndToppings_SumsCorrectly()
    {
        // Multiple + Varied
        var p1 = P(size: 9, basePrice: 8.00m);                     // plain small
        var p2 = P(size: 17, basePrice: 12.00m, 0.50m, 0.50m);     // large with 2 tops
        var p3 = P(size: 12, basePrice: 10.00m, 1.00m);            // medium with 1 top
        var p4 = P(size: 12, basePrice: 10.00m);                   // medium plain

        var (order, expected, fmt) = MakeOrder(new List<Pizza> { p1, p2, p3, p4 });

        var total = order.GetTotalPrice();
        var formatted = order.GetFormattedTotalPrice();

        TestContext.WriteLine(
            $"Multiple+Varied → Totals: [{ExpectedPizzaTotal(p1):0.00}, {ExpectedPizzaTotal(p2):0.00}, {ExpectedPizzaTotal(p3):0.00}, {ExpectedPizzaTotal(p4):0.00}] Sum={total:0.00}"
        );
        Assert.That(total, Is.EqualTo(expected));
        Assert.That(formatted, Is.EqualTo(fmt));
    }

    // ---------- Propagation/negative checks ----------
    [Test]
    public void OrderTotal_Throws_WhenContainedPizzaHasNullSpecial()
    {
        var good = P(size: 12, basePrice: 10.00m, 1.00m);
        var bad = new Pizza { Size = 12, Special = null! }; // GetBasePrice throws

        var order = new Order { Pizzas = new List<Pizza> { good, bad } };

        var ex = Assert.Throws<NullReferenceException>(() => order.GetTotalPrice());
        TestContext.WriteLine($"Propagation (null Special) → Threw {ex?.GetType().Name}: '{ex?.Message}'");
        StringAssert.Contains("Special", ex!.Message);
    }
}