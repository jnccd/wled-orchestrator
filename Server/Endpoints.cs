using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Server.Helper;
using Server.Services.DataStore;
using Server.Services.WledCommunicator;
namespace Server;

public static class WledOrchestratorEndpoints
{
    public static void RegisterEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseCors(policy => policy.AllowAnyOrigin());
        }
        app.UseDefaultFiles(new DefaultFilesOptions
        {
            DefaultFileNames = ["index.html"],
            FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "..", "Frontend", "dist")),
        });
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "..", "Frontend", "dist")),
        });

        var routes = (IEndpointRouteBuilder)app;

        app.MapGet("/hewwo", () =>
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

        app.MapGet("/wledServers", (
            [FromServices] IWledCommunicatorService wledCommunicator) =>
            Results.Json(wledCommunicator.Leds.Select(x => x.Address).ToArray()));

        app.MapGet("/state", (
            [FromServices] IDataStoreService dataStore) =>
            Results.Text(JsonSerializer.Serialize(dataStore.Data), contentType: "application/json"));

        app.MapPost("/state", async (
            [FromServices] IDataStoreService dataStore,
            HttpRequest request) =>
        {
            using StreamReader bodyStream = new(request.Body);
            string body = await bodyStream.ReadToEndAsync();
            dataStore.LoadFrom(body);
        });
    }
}
