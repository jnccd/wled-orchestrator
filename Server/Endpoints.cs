using System.ComponentModel.DataAnnotations;
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

        app.MapGet("/wledServers", [ProducesResponseType(typeof(string[]), 200)] (
            [FromServices] WledCommunicatorService wledCommunicator) =>
            Results.Json(wledCommunicator.Leds.Select(x => x.Address).ToArray()));

        app.MapGet("/state", [ProducesResponseType(typeof(DataStoreRoot), 200)] (
            [FromServices] DataStoreService dataStore) =>
            Results.Json(dataStore.Data));

        app.MapPut("/state", (
            [FromServices] DataStoreService dataStore,
            [FromBody, Required] DataStoreRoot newState) =>
        {
            try
            {
                dataStore.LoadFrom(JsonSerializer.Serialize(newState));
                dataStore.Save();
            }
            catch
            {
                // TODO: Throw exception to user?
            }
        });
    }
}
