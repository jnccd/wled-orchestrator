using System.Text.Json;

namespace Server.Helper;

public static partial class Extensions
{
    public static void Print(this object toExamine)
    {
        Console.WriteLine($"{JsonSerializer.Serialize(toExamine)}");
    }
}
