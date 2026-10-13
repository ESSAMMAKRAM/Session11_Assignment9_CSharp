// =====================================================================
//  Section 02: Order Processing System (model, delegates, pricing,
//  validation, processing)
// =====================================================================

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    // Extra property: filled by OrderService after the price is calculated,
    // so the event handlers can read the final total.
    public decimal TotalPrice { get; set; }
}

// Part 1: user-defined delegate
public delegate decimal PriceCalculator(Order order);

// Part 1: methods that match the PriceCalculator delegate
public static class DelegatePricing
{
    // Price x Quantity
    public static decimal CalculateTotal(Order order)
    {
        return order.Price * order.Quantity;
    }

    // Price x Quantity - discount (discount = 10% of the total)
    public static decimal CalculateTotalWithDiscount(Order order)
    {
        decimal total = order.Price * order.Quantity;
        decimal discount = total * 0.10m;
        return total - discount;
    }

    public static decimal CalculateOrderPrice(Order order, PriceCalculator calculator)
    {
        return calculator(order);
    }
}

// Part 2: the same idea with the built-in Func<Order, decimal>.
// (It lives in a separate class on purpose: if both overloads were in one class,
// passing a method group or a lambda would be ambiguous, because PriceCalculator
// and Func<Order, decimal> are two different delegate types with the same signature.)
public static class FuncPricing
{
    public static decimal CalculateOrderPrice(Order order, Func<Order, decimal> calculator)
    {
        return calculator(order);
    }
}

// Bonus: pricing strategies supplied from outside as Func<Order, decimal>
public static class PricingStrategies
{
    public static readonly Func<Order, decimal> Normal =
        order => order.Price * order.Quantity;

    public static readonly Func<Order, decimal> TenPercentOff =
        order => order.Price * order.Quantity * 0.90m;

    public static readonly Func<Order, decimal> TwentyPercentOff =
        order => order.Price * order.Quantity * 0.80m;

    public static readonly Func<Order, decimal> Vip =
        order => order.Price * order.Quantity * 0.70m;
}

// Part 3: validation with Predicate<Order>
public static class OrderValidator
{
    public static bool ValidateOrder(Order order, Predicate<Order> validationRule)
    {
        return validationRule(order);
    }
}

// Part 4: run any action after the order is processed
public static class OrderProcessor
{
    public static void ProcessOrder(Order order, Action<Order> action)
    {
        Console.WriteLine($"Processing order {order.Id}...");
        action(order);
    }
}
