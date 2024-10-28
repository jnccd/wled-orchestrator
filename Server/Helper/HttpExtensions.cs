using System.Diagnostics;
using System.Net.Mime;
using System.Text;

namespace Server.Helper;

public static partial class Extensions
{
    public static void PrintReqHeaders(HttpRequest request)
    {
        Debug.WriteLine("Req: " + request.ToString());
        for (int i = 0; i < request.Headers.Keys.Count; i++)
            Debug.WriteLine(request.Headers.Keys.ElementAt(i).ToString() + ": " +
                request.Headers.GetCommaSeparatedValues(request.Headers.Keys.ElementAt(i)).Aggregate((x, y) => x + "~~" + y));
    }
    public static IResult Html(this IResultExtensions resultExtensions, string html)
    {
        ArgumentNullException.ThrowIfNull(resultExtensions);

        return new HtmlResult(html);
    }
    class HtmlResult(string html) : IResult
    {
        private readonly string _html = html;

        public Task ExecuteAsync(HttpContext httpContext)
        {
            httpContext.Response.ContentType = MediaTypeNames.Text.Html;
            httpContext.Response.ContentLength = Encoding.UTF8.GetByteCount(_html);
            return httpContext.Response.WriteAsync(_html);
        }
    }
    public static async Task<string> GetHttpResponseFrom(this string url, int timeout = 5)
    {
        using var client = new HttpClient();
        client.Timeout = new TimeSpan(0, 0, timeout);

        using HttpResponseMessage response = await client.GetAsync(url);
        string responseText = await response.Content.ReadAsStringAsync();

        return responseText;
    }
    public static async void HttpPostAsJsonTo(this string json, string address, int timeout = 5)
    {
        using var client = new HttpClient();
        client.Timeout = new TimeSpan(0, 0, timeout);

        var contentData = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var res = await client.PostAsync(address, contentData);
        }
        catch (TaskCanceledException) { }
    }
}
