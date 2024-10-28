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
    public static string Combine(this IEnumerable<string> s, string combinator = "") =>
        !s.Any() ? "" : s.Aggregate("", (x, y) => x + combinator + y).Remove(0, combinator.Length);
    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self) =>
        self.Select((item, index) => (item, index));

}
