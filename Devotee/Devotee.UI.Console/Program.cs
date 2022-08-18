using System.Text;

namespace Devotee.UI.Console;

internal static class Program
{
    public static async Task Main()
    {
        System.Console.InputEncoding = Encoding.Unicode;
        System.Console.OutputEncoding = Encoding.Unicode;
        await new ProgramContext().Main();
    }
}