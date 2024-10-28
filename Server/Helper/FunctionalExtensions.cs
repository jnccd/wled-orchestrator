using System.Diagnostics;
using System.Net.Mime;
using System.Text;

namespace Server.Helper;

public static partial class Extensions
{
    public static bool ContainsAll(this string s, string[] contains)
    {
        foreach (string t in contains)
            if (!s.Contains(t))
                return false;
        return true;
    }
    public static string Combine(this IEnumerable<string> s, string combinator = "")
    {
        return !s.Any() ? "" : s.Aggregate("", (x, y) => x + combinator + y).Remove(0, combinator.Length);
    }
}
