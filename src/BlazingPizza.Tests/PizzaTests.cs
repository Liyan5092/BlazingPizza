using BlazingPizza.Shared;
using NUnit.Framework.Internal;

namespace BlazingPizza.Tests;

public class PizzaTests
{
    // We will be testing the functionality of Pizza.GetFormattedTotalPrice() method

    [TestFixture]
    public class PizzaPricingPairwiseTests
    {
        // Representative specials grouped by price tier (Low / Medium / High)
        private static readonly PizzaSpecial LowSpecial = new()
        {
            Name = "Margherita",
            BasePrice = 9.99m
        };

        private static readonly PizzaSpecial MediumSpecial = new()
        {
            Name = "Classic pepperoni",
            BasePrice = 10.50m
        };

        private static readonly PizzaSpecial HighSpecial = new()
        {
            Name = "Buffalo chicken",
            BasePrice = 12.75m
        };

        // Helper: build a toppings list of a given count, each with the same price
        private static List<PizzaTopping> MakeToppings(int count, decimal toppingPrice = 0.50m)
        {
            var list = new List<PizzaTopping>(capacity: Math.Max(0, count));
            for (int i = 0; i < count; i++)
            {
                list.Add(new PizzaTopping
                {
                    Topping = new Topping
                    {
                        Name = $"T{i + 1}",
                        Price = toppingPrice
                    }
                });
            }
            return list;
        }

        // Helper: construct a pizza and compute expected values (decimal + formatted string)
        private static (Pizza pizza, decimal expectedTotal, string expectedFormatted)
            BuildCase(PizzaSpecial special, int size, int toppingsCount, decimal toppingPrice = 0.50m)
        {
            var pizza = new Pizza
            {
                Special = special,
                Size = size,
                Toppings = MakeToppings(toppingsCount, toppingPrice)
            };

            // Expected: ((size / DefaultSize) * base) + sum(topping prices)
            decimal expectedBase = ((decimal)size / Pizza.DefaultSize) * special.BasePrice;
            decimal expectedTotal = expectedBase + (toppingsCount * toppingPrice);
            string expectedFormatted = expectedTotal.ToString("0.00");

            return (pizza, expectedTotal, expectedFormatted);
        }

        // Pairwise table (9 rows) -> mapped to real values
        // P1: Special  = { Low, Medium, High }  -> LowSpecial, MediumSpecial, HighSpecial
        // P2: Size     = { 9, 12, 17 }
        // P3: Toppings = { 0, 2, 6 }
        public static IEnumerable<TestCaseData> PairwiseCases()
        {
            // 1 | Low | 9  | 0
            {
                var (pizza, total, fmt) = BuildCase(LowSpecial, 9, 0);
                yield return new TestCaseData(pizza, total, fmt)
                    .SetName("PWC_Low_Size9_Top0");
            }
            // 2 | Low | 12 | 2
            {
                var (pizza, total, fmt) = BuildCase(LowSpecial, 12, 2);
                yield return new TestCaseData(pizza, total, fmt)
                    .SetName("PWC_Low_Size12_Top2");
            }
            // 3 | Low | 17 | 6
            {
                var (pizza, total, fmt) = BuildCase(LowSpecial, 17, 6);
                yield return new TestCaseData(pizza, total, fmt)
                    .SetName("PWC_Low_Size17_Top6");
            }
            // 4 | Medium | 9  | 2
            {
                var (pizza, total, fmt) = BuildCase(MediumSpecial, 9, 2);
                yield return new TestCaseData(pizza, total, fmt)
                    .SetName("PWC_Medium_Size9_Top2");
            }
            // 5 | Medium | 12 | 6
            {
                var (pizza, total, fmt) = BuildCase(MediumSpecial, 12, 6);
                yield return new TestCaseData(pizza, total, fmt)
                    .SetName("PWC_Medium_Size12_Top6");
            }
            // 6 | Medium | 17 | 0
            {
                var (pizza, total, fmt) = BuildCase(MediumSpecial, 17, 0);
                yield return new TestCaseData(pizza, total, fmt)
                    .SetName("PWC_Medium_Size17_Top0");
            }
            // 7 | High | 9  | 6
            {
                var (pizza, total, fmt) = BuildCase(HighSpecial, 9, 6);
                yield return new TestCaseData(pizza, total, fmt)
                    .SetName("PWC_High_Size9_Top6");
            }
            // 8 | High | 12 | 0
            {
                var (pizza, total, fmt) = BuildCase(HighSpecial, 12, 0);
                yield return new TestCaseData(pizza, total, fmt)
                    .SetName("PWC_High_Size12_Top0");
            }
            // 9 | High | 17 | 2
            {
                var (pizza, total, fmt) = BuildCase(HighSpecial, 17, 2);
                yield return new TestCaseData(pizza, total, fmt)
                    .SetName("PWC_High_Size17_Top2");
            }
        }

        [TestCaseSource(nameof(PairwiseCases))]
        public void GetTotalPrice_Pairwise_ReturnsExpected(Pizza pizza, decimal expectedTotal, string _)
        {
            var total = pizza.GetTotalPrice();
            TestContext.WriteLine($"[GetTotalPrice - {TestContext.CurrentContext.Test.Name}] " +
                          $"Special={pizza.Special?.Name} Base={pizza.Special?.BasePrice} " +
                          $"Size={pizza.Size} Toppings={pizza.Toppings.Count} " +
                          $"Expected={expectedTotal:0.00} Actual={total:0.00}");
            Assert.That(total, Is.EqualTo(expectedTotal));
        }

        [TestCaseSource(nameof(PairwiseCases))]
        public void GetFormattedTotalPrice_Pairwise_ReturnsExpectedString(Pizza pizza, decimal _, string expectedFormatted)
        {
            var formatted = pizza.GetFormattedTotalPrice();
            TestContext.WriteLine($"[GetFormattedTotalPrice - {TestContext.CurrentContext.Test.Name}] " +
                        $"Formatted Expected={expectedFormatted} Actual={formatted} " +
                        $"(Special={pizza.Special?.Name}, Size={pizza.Size}, Tops={pizza.Toppings.Count})");
            Assert.That(formatted, Is.EqualTo(expectedFormatted));
        }

        // -------- Negative / exception behavior checks --------
        [Test]
        public void GetBasePrice_Throws_WhenSpecialIsNull()
        {
            var pizza = new Pizza
            {
                Size = Pizza.DefaultSize,
                Special = null!
            };

            var ex = Assert.Throws<NullReferenceException>(() => pizza.GetBasePrice());

            TestContext.WriteLine($"[{TestContext.CurrentContext.Test.Name}] " +
                          $"Size={pizza.Size}, Special=null → Threw: {ex?.GetType().Name} " +
                          $"Message='{ex?.Message}'");
        }

        [Test]
        public void GetTotalPrice_Throws_WhenAnyToppingIsNull()
        {
            var pizza = new Pizza
            {
                Size = Pizza.DefaultSize,
                Special = LowSpecial,
                Toppings = new List<PizzaTopping>
                {
                    new PizzaTopping { Topping = new Topping { Name = "OK", Price = 0.50m } },
                    new PizzaTopping { Topping = null } // triggers the guard in GetTotalPrice()
                }
            };

            var ex = Assert.Throws<NullReferenceException>(() => pizza.GetTotalPrice());

            TestContext.WriteLine($"[{TestContext.CurrentContext.Test.Name}] " +
                                  $"Special={pizza.Special?.Name} Base={pizza.Special?.BasePrice:0.00} " +
                                  $"Size={pizza.Size} Tops={pizza.Toppings.Count} " +
                                  $"→ Threw: {ex?.GetType().Name} Message='{ex?.Message}'");
        }
    }
}