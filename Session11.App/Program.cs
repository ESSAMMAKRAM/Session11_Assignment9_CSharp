using System.Globalization;

// Same number/date format on every machine
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

// =====================================================================
//  SECTION 01: Books - passing BookFunctions methods to ProcessBooks
// =====================================================================
Console.WriteLine("########## SECTION 01: Books ##########");

List<Book> books = new()
{
    new Book("978-0132350884", "Clean Code", new[] { "Robert C. Martin" },
             new DateTime(2008, 8, 1), 45.50m),
    new Book("978-0201633610", "Design Patterns",
             new[] { "Erich Gamma", "Richard Helm", "Ralph Johnson", "John Vlissides" },
             new DateTime(1994, 10, 31), 54.99m),
    new Book("978-0135957059", "The Pragmatic Programmer", new[] { "Andrew Hunt", "David Thomas" },
             new DateTime(2019, 9, 13), 49.95m)
};

Console.WriteLine("\n--- Book.ToString() ---");
foreach (Book book in books)
{
    Console.WriteLine(book);
}

// Case 1: user-defined delegate (BookFunctionDelegate)
Console.WriteLine("\n--- Case 1: user-defined delegate -> GetTitle, GetAuthors ---");
BookFunctionDelegate titleFn = BookFunctions.GetTitle;
LibraryEngine.ProcessBooks(books, titleFn);
BookFunctionDelegate authorsFn = BookFunctions.GetAuthors;
LibraryEngine.ProcessBooks(books, authorsFn);

// Case 2: the proper built-in delegate (Func<Book, string>)
Console.WriteLine("\n--- Case 2: built-in Func<Book, string> -> GetPrice ---");
Func<Book, string> priceFn = BookFunctions.GetPrice;
LibraryEngine.ProcessBooks(books, priceFn);

// Case 3: anonymous method (GetISBN)
Console.WriteLine("\n--- Case 3: anonymous method -> GetISBN ---");
BookFunctionDelegate isbnFn = delegate (Book B)
{
    return B.ISBN;
};
LibraryEngine.ProcessBooks(books, isbnFn);

// Case 4: lambda expression (GetPublicationDate)
Console.WriteLine("\n--- Case 4: lambda expression -> GetPublicationDate ---");
Func<Book, string> dateFn = B => B.PublicationDate.ToString("yyyy-MM-dd");
LibraryEngine.ProcessBooks(books, dateFn);

// =====================================================================
//  SECTION 02: Order Processing System
// =====================================================================
Console.WriteLine("\n\n########## SECTION 02: Orders ##########");

Order order = new() { Id = 1, CustomerName = "Ahmed", Price = 100m, Quantity = 3 };

// ---------------------------------------------------------------------
// Part 1: user-defined delegate (PriceCalculator)
// ---------------------------------------------------------------------
Console.WriteLine("\n===== Part 1: User-defined delegate =====");

decimal normalTotal = DelegatePricing.CalculateOrderPrice(order, DelegatePricing.CalculateTotal);
decimal discountedTotal = DelegatePricing.CalculateOrderPrice(order, DelegatePricing.CalculateTotalWithDiscount);
Console.WriteLine($"CalculateTotal             = {normalTotal:F2}");
Console.WriteLine($"CalculateTotalWithDiscount = {discountedTotal:F2}");

// ---------------------------------------------------------------------
// Part 2: Func<Order, decimal> with lambdas
// ---------------------------------------------------------------------
Console.WriteLine("\n===== Part 2: Func<> =====");

decimal funcTotal = FuncPricing.CalculateOrderPrice(order, x => x.Price * x.Quantity);
decimal funcDiscounted = FuncPricing.CalculateOrderPrice(order, x => x.Price * x.Quantity * 0.85m);
Console.WriteLine($"Lambda: Price x Quantity          = {funcTotal:F2}");
Console.WriteLine($"Lambda: Price x Quantity - 15%    = {funcDiscounted:F2}");

// Question: What problem does Func<> solve compared with creating a custom delegate?
// Answer: With a custom delegate we must declare a new delegate type for every signature
// (PriceCalculator, then another one for the next signature, and so on). Func<> is a ready-made
// generic delegate that fits almost any "takes inputs, returns a value" signature, so we write
// less boilerplate and every API that uses Func<> accepts our lambdas and methods directly,
// without forcing everybody to learn or share our custom type.

// ---------------------------------------------------------------------
// Part 3: Predicate<Order> validation rules
// ---------------------------------------------------------------------
Console.WriteLine("\n===== Part 3: Predicate<> =====");

Predicate<Order> hasQuantity = o => o.Quantity > 0;
Predicate<Order> hasPrice = o => o.Price > 0;
Predicate<Order> hasCustomer = o => !string.IsNullOrWhiteSpace(o.CustomerName);

Order badOrder = new() { Id = 99, CustomerName = "", Price = 50m, Quantity = 0 };

Console.WriteLine($"Good order -> Quantity > 0: {OrderValidator.ValidateOrder(order, hasQuantity)}");
Console.WriteLine($"Good order -> Price > 0   : {OrderValidator.ValidateOrder(order, hasPrice)}");
Console.WriteLine($"Good order -> Has customer: {OrderValidator.ValidateOrder(order, hasCustomer)}");
Console.WriteLine($"Bad order  -> Quantity > 0: {OrderValidator.ValidateOrder(badOrder, hasQuantity)}");
Console.WriteLine($"Bad order  -> Has customer: {OrderValidator.ValidateOrder(badOrder, hasCustomer)}");

// Question: Why is Predicate<Order> more expressive than Func<Order, bool>?
// Answer: Both have the same shape, but Predicate<Order> says WHAT the function is for:
// it is a test/rule about an order ("does this order satisfy the condition?"). A reader
// instantly knows it is a yes/no check. Func<Order, bool> only says "some function that
// returns a bool", which could mean anything (IsPaid, HasStock, SaveSucceeded...).
// The name also matches the .NET APIs built around conditions (List.Find, Exists, RemoveAll).

// ---------------------------------------------------------------------
// Part 4: Action<Order> - pass different behaviors to the same method
// ---------------------------------------------------------------------
Console.WriteLine("\n===== Part 4: Action<> =====");

Action<Order> printOrder = o => Console.WriteLine($"  Order {o.Id} processed.");
Action<Order> sendConfirmation = o => Console.WriteLine($"  Confirmation message sent to {o.CustomerName}.");
Action<Order> writeAudit = o => Console.WriteLine($"  Audit: order {o.Id}, quantity {o.Quantity}, price {o.Price:F2}.");

OrderProcessor.ProcessOrder(order, printOrder);
OrderProcessor.ProcessOrder(order, sendConfirmation);
OrderProcessor.ProcessOrder(order, writeAudit);

// Multicast delegate: one Action that runs all three behaviors
Console.WriteLine("\n--- Multicast delegate (3 actions combined with +=) ---");
Action<Order> allActions = printOrder;
allActions += sendConfirmation;
allActions += writeAudit;
OrderProcessor.ProcessOrder(order, allActions);

// ---------------------------------------------------------------------
// Part 5: Events - several handlers subscribed to OrderProcessed
// ---------------------------------------------------------------------
Console.WriteLine("\n===== Part 5: Events =====");

OrderService orderService = new(PricingStrategies.Normal);
orderService.OrderProcessed += OrderHandlers.PrintMessage;
orderService.OrderProcessed += OrderHandlers.SendNotification;
orderService.OrderProcessed += OrderHandlers.WriteAudit;

orderService.ProcessOrder(order);

// ---------------------------------------------------------------------
// Part 6: Unsubscribing
// ---------------------------------------------------------------------
Console.WriteLine("\n===== Part 6: Unsubscribe =====");

orderService.OrderProcessed -= OrderHandlers.PrintMessage;
Console.WriteLine("(PrintMessage was unsubscribed - it must NOT appear below)");
orderService.ProcessOrder(order);

// ---------------------------------------------------------------------
// Part 7: Final application
//   Order -> Validate -> Calculate Price -> Process -> OrderProcessed -> Handlers
// ---------------------------------------------------------------------
Console.WriteLine("\n===== Part 7: Final application =====");

List<Order> orders = new()
{
    new Order { Id = 101, CustomerName = "Sara", Price = 200m, Quantity = 2 },
    new Order { Id = 102, CustomerName = "", Price = 150m, Quantity = 1 },     // invalid: no customer
    new Order { Id = 103, CustomerName = "Omar", Price = 80m, Quantity = 0 },  // invalid: quantity 0
    new Order { Id = 104, CustomerName = "Mona", Price = 120m, Quantity = 5 }
};

Predicate<Order>[] rules = { hasQuantity, hasPrice, hasCustomer };

OrderService finalService = new(PricingStrategies.TenPercentOff);
finalService.OrderProcessed += OrderHandlers.PrintMessage;
finalService.OrderProcessed += OrderHandlers.SendNotification;
finalService.OrderProcessed += OrderHandlers.WriteAudit;

// Validate (no if/else: invalid orders are simply filtered out)
List<Order> validOrders = orders
    .Where(o => rules.All(rule => OrderValidator.ValidateOrder(o, rule)))
    .ToList();

orders.Except(validOrders).ToList()
      .ForEach(o => Console.WriteLine($"Order {o.Id} skipped: failed validation."));

// Calculate price + process + raise the event for every valid order
validOrders.ForEach(finalService.ProcessOrder);

// ---------------------------------------------------------------------
// Bonus: pricing strategies supplied at runtime
// ---------------------------------------------------------------------
Console.WriteLine("\n===== Bonus: Pricing strategies =====");

Dictionary<string, Func<Order, decimal>> strategies = new()
{
    ["Normal price"] = PricingStrategies.Normal,
    ["10% discount"] = PricingStrategies.TenPercentOff,
    ["20% discount"] = PricingStrategies.TwentyPercentOff,
    ["VIP discount"] = PricingStrategies.Vip
};

foreach (var (name, strategy) in strategies)
{
    Console.WriteLine($"Strategy: {name}");
    OrderService service = new(strategy);   // OrderService never changes
    service.ProcessOrder(order);
}

Console.WriteLine("\nDone.");

// =====================================================================
//  ANSWERS (Q1 - Q8)
// =====================================================================

// Q1: PriceCalculator vs Func<Order, decimal>
// Both describe "a method that takes an Order and returns a decimal". PriceCalculator is a
// delegate type WE declared (custom, with a meaningful name, but one more type to maintain).
// Func<Order, decimal> is the built-in generic delegate, so nothing needs to be declared.
// They are different types: a PriceCalculator variable cannot be assigned directly to a
// Func<Order, decimal> variable, even though the signatures are identical.

// Q2: Action<Order> vs Func<Order, decimal>
// Action<Order> takes an Order and returns nothing (void): it DOES something (print, send,
// save...) - a side effect. Func<Order, decimal> takes an Order and returns a decimal: it
// COMPUTES a value. Action = behavior/command, Func = calculation/query.

// Q3: Why does Pre
