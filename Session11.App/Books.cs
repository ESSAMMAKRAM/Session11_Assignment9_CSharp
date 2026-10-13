// =====================================================================
//  Section 01: Book, BookFunctions, LibraryEngine
// =====================================================================

public class Book
{
    public string ISBN { get; set; }
    public string Title { get; set; }
    public string[] Authors { get; set; }
    public DateTime PublicationDate { get; set; }
    public decimal Price { get; set; }

    public Book(string _ISBN, string _Title,
        string[] _Authors, DateTime _PublicationDate,
        decimal _Price)
    {
        ISBN = _ISBN;
        Title = _Title;
        Authors = _Authors;
        PublicationDate = _PublicationDate;
        Price = _Price;
    }

    public override string ToString()
    {
        return $"ISBN: {ISBN} | Title: {Title} | Authors: {string.Join(", ", Authors)} | " +
               $"Published: {PublicationDate:yyyy-MM-dd} | Price: {Price:F2}";
    }
}

public class BookFunctions
{
    public static string GetTitle(Book B)
    {
        return B.Title;
    }

    public static string GetAuthors(Book B)
    {
        return string.Join(", ", B.Authors);
    }

    public static string GetISBN(Book B)
    {
        return B.ISBN;
    }

    public static string GetPublicationDate(Book B)
    {
        return B.PublicationDate.ToString("yyyy-MM-dd");
    }

    public static string GetPrice(Book B)
    {
        return B.Price.ToString("F2");
    }
}

// Case 1: user-defined delegate with the same signature as the BookFunctions methods
public delegate string BookFunctionDelegate(Book B);

public class LibraryEngine
{
    // Version 1: accepts the user-defined delegate
    public static void ProcessBooks(List<Book> bList, BookFunctionDelegate fPtr)
    {
        foreach (Book B in bList)
        {
            Console.WriteLine(fPtr(B));
        }
    }

    // Version 2: accepts the proper built-in delegate (Func<Book, string>)
    public static void ProcessBooks(List<Book> bList, Func<Book, string> fPtr)
    {
        foreach (Book B in bList)
        {
            Console.WriteLine(fPtr(B));
        }
    }
}
