using System;
using System.Text;

// PrintedPage is a value type
struct PrintedPage
{
    public string Text;
}

// WebPage is a reference type
class WebPage
{
    public string Text;
}

class Demonstration
{
    static void Main()
    {
        string test = Console.ReadLine();
        Console.WriteLine(test);
        test = test.Remove(test.Length - 1);
        Console.WriteLine(test);
    }
}