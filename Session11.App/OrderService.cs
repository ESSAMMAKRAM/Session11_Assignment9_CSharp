// =====================================================================
//  Part 5 + Bonus: OrderService (publisher) and the event handlers
// =====================================================================

public class OrderService
{
    // The pricing behavior is received from outside (Bonus):
    // OrderService does NOT know how the price is calculated.
    private readonly Func<Order, decimal> _pricingStrategy;

    // Part 5: the event raised when an order is completed
    public event Action<Order>? OrderProcessed;

    public OrderService(Func<Order, decimal> pricingStrategy)
    {
        _pricingStrategy = pricingStrategy;
    }

    public void ProcessOrder(Order order)
    {
        // Calculate Price (using the supplied strategy)
        order.TotalPrice = _pricingStrategy(order);

        // Process Order
        Console.WriteLine($"Order {order.Id} processed. Total = {order.TotalPrice:F2}");

        // Notify every subscriber
        OrderProcessed?.Invoke(order);
    }
}

// Handlers that Program subscribes to the OrderProcessed event
public static class OrderHandlers
{
    public static void PrintMessage(Order order)
    {
        Console.WriteLine($"  [Print]        Order {order.Id} for {order.CustomerName} is completed.");
    }

    public static void SendNotification(Order order)
    {
        Console.WriteLine($"  [Notification] Confirmation sent to {order.CustomerName} for order {order.Id}.");
    }

    public static void WriteAudit(Order order)
    {
        Console.WriteLine($"  [Audit]        Order {order.Id}, total {order.TotalPrice:F2}, recorded at {DateTime.Now:HH:mm:ss}.");
    }
}
