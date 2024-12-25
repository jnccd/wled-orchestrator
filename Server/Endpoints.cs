using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Server.Helper;
using Server.Services.DataStore;
using Server.Services.DataStore.Types;
using Server.Services.WledCommunicator;
namespace Server;

public static class WledOrchestratorEndpoints
{
    public static void RegisterEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseCors(policy => policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
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
            dataStore.LoadFrom(JsonSerializer.Serialize(newState));
            dataStore.Save();
        });

        app.MapPost("/state/createGroup", (
            [FromServices] DataStoreService dataStore,
            string? idOfFirstGroupSegment) =>
        {
            (var segmentGroup, var segment) = dataStore.Data.Groups.Select(g => (g, g.LedSegments.Where(x => x.ReadonlyId == idOfFirstGroupSegment).FirstOrDefault())).FirstOrDefault();
            if (segment != null)
                segmentGroup.LedSegments.Remove(segment);

            var newGroup = new LedSegmentGroup("New Segment", segment == null ? [] : [segment], null, new(255, 255, 255));
            dataStore.Data.Groups.Add(newGroup);
            dataStore.Save();

            return newGroup;
        });

        app.MapPut("/state/moveSegment", (
            [FromServices] DataStoreService dataStore,
            [Required] string segmentId,
            string? targetGroupId) =>
        {
            (var segmentGroup, var segment) = dataStore.Data.Groups
                .Select(g => (g, g.LedSegments.FirstOrDefault(x => x.ReadonlyId == segmentId)))
                .Where(x => x.Item2 != null)
                .FirstOrDefault();
            if (segmentGroup == null || segment == null)
                return Results.NotFound("The SegmentId was not found in any groups");

            var targetGroup = targetGroupId == null ? null : dataStore.Data.Groups.FirstOrDefault(x => x.Id == Guid.Parse(targetGroupId));
            if (targetGroup == null)
            {
                targetGroup = new LedSegmentGroup("New Group", [], null, new(255, 255, 255));
                dataStore.Data.Groups.Add(targetGroup);
            }

            segmentGroup.LedSegments.Remove(segment);
            targetGroup.LedSegments.Add(segment);
            if (segmentGroup.LedSegments.Count == 0)
                dataStore.Data.Groups.Remove(segmentGroup);

            dataStore.Save();

            return Results.Accepted();
        });
    }
}
