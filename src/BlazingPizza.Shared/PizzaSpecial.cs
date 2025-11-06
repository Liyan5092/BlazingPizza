namespace BlazingPizza.Shared;

/// <summary>
/// Represents a pre-configured template for a pizza a user can order
/// </summary>
public class PizzaSpecial
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal BasePrice { get; set; }

    public string Description { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = "img/pizzas/cheese.jpg";

    public string GetFormattedBasePrice() => BasePrice.ToString("0.00");

    //public PizzaSpecial(string name, decimal basePrice)
    //{
    //    Name = name;
    //    BasePrice = basePrice;
    //}
}