//using BlazingPizza.Shared;
//using System.Drawing;

//namespace BlazingPizza.Tests
//{
//    public class Tests
//    {

//        [SetUp]
//        public void Setup()
//        {
//            Pizza pizza = new();
//        }

//        [Test]
//        public void Test1()
//        {
//            Assert.Pass();
//        }

//        [Test]
//        public void Test2()
//        {
//            Assert.Fail();
//        }

//        [Test]
//        public void GetBasePriceTest()
//        {
//            Pizza pizza = new Pizza()
//            {
//                OrderId = 1,
//                Size = 12,
//                Special = new PizzaSpecial()
//                {
//                    BasePrice = 10.0m,
//                    Name = "Test Special",
//                    Description = "A special for testing",
//                },
//                Toppings = new()
//                {
//                    new PizzaTopping()
//                    {
//                        Topping = new Topping()
//                        {
//                            Name = "Test Topping 1",
//                            Price = 1.0m,
//                        }
//                    },
//                    new PizzaTopping()
//                    {
//                        Topping = new Topping()
//                        {
//                            Name = "Test Topping 2",
//                            Price = 1.5m,
//                        }
//                    }
//                }
//            };

//            pizza.GetBasePrice();
//            if(pizza.GetBasePrice() != 10.0m)
//            {
//                Assert.Fail("GetBasePrice did not return the expected value.");
//            }
//            else
//            {
//                Assert.Pass("GetBasePrice returned the expected value.");
//            }
//        }
//    }
//}