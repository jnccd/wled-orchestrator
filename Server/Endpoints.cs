using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Server.Helper;
using Server.Services.WledCommunicator;
namespace Server;

public static class WledOrchestratorEndpoints
{
    public static void RegisterEndpoints(this WebApplication webApp, IServiceProvider services)
    {
        webApp.UseDefaultFiles(new DefaultFilesOptions
        {
            DefaultFileNames = ["index.html"],
            FileProvider = new PhysicalFileProvider(Path.Combine(webApp.Environment.ContentRootPath, "..", "Frontend", "dist")),
        });
        webApp.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(webApp.Environment.ContentRootPath, "..", "Frontend", "dist")),
        });

        var routes = (IEndpointRouteBuilder)webApp;
        webApp.MapGet("/hewwo", () =>
        {
            return Results.Extensions.Html(@$"<!doctype html>
                <html>
                    <head>
                        <title>Hewwo</title>
                        <style>
                            body {{font-family: sans-serif;}}
                        </style>
                    </head>
                    <body>
                        <h1>Hewwo Wowld :3</h1>
                    </body>
                </html>");
        });
        webApp.MapGet("/wledServers", (
            [FromServices] IWledCommunicatorService wledCommunicator,
            HttpRequest request) =>
            Results.Json(wledCommunicator.Leds.Select(x => x.address).ToArray()));
    }
}
