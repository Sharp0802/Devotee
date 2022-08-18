namespace Devotee.UI.Console;

public static class ConsoleHelper
{
    public static void CombinePrint(string a, string b)
    {
        var allA = a.Split('\n');
        var allB = b.Split('\n');

        for (var i = 0; i < Math.Max(allA.Length, allB.Length); ++i)
        {
            var ai = allA.ElementAtOrDefault(i) ?? string.Empty;
            var bi = allB.ElementAtOrDefault(i) ?? string.Empty;

            WriteAt(ai, 0);
            WriteAt(bi, System.Console.BufferWidth / 2 + 1);
            System.Console.WriteLine();
        }
    }

    public static void WriteAt(string str, int at)
    {
        var (_, top) = System.Console.GetCursorPosition();
        System.Console.SetCursorPosition(at, top);
        System.Console.Write(str);
    }
}