using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Server.Services.DataStore;
using Server.Services.DataStore.Types;
using Server.Services.LedTheme;

namespace Server.Endpoints;

public static class RegisterEndpoints
{
    public static void RegisterWledEndpoints(this WebApplication app)
    {
        app.MapGet("/state", [ProducesResponseType(typeof(DataStoreRoot), 200)] (
            [FromServices] DataStoreService dataStore) =>
            Results.Json(dataStore.Data));

        app.MapPut("/state/group/rename", (
            [FromServices] DataStoreService dataStore,
            [Required] string groupId,
            [Required] string newName) =>
        {
            lock (dataStore.lockject)
            {
                var group = dataStore.Data.Groups.FirstOrDefault(x => x.Id == Guid.Parse(groupId));
                if (group == null)
                    return Results.NotFound("The GroupId was not found in any groups");

                group.Name = newName;

                dataStore.Save();
            }

            return Results.Accepted();
        });

        app.MapPut("/state/group/theme", (
            [FromServices] DataStoreService dataStore,
            [Required] string groupId,
            [FromBody, Required] LedTheme newTheme) =>
        {
            lock (dataStore.lockject)
            {
                var group = dataStore.Data.Groups.FirstOrDefault(x => x.Id == Guid.Parse(groupId));
                if (group == null)
                    return Results.NotFound("The GroupId was not found in any groups");

                group.Theme = newTheme;

                dataStore.Save();
            }

            return Results.Accepted();
        });

        app.MapDelete("/state/group", (
            [FromServices] DataStoreService dataStore,
            [Required] string groupId) =>
        {
            lock (dataStore.lockject)
            {
                if (dataStore.Data.Groups.Count == 1)
                    return Results.BadRequest("This is the last group!");
                var group = dataStore.Data.Groups.FirstOrDefault(x => x.Id == Guid.Parse(groupId));
                if (group == null)
                    return Results.NotFound("The GroupId was not found in any groups!");
                if (group.LedSegments.Count > 0)
                    return Results.BadRequest("The group is not empty!");

                dataStore.Data.Groups.Remove(group);

                dataStore.Save();
            }

            return Results.NoContent();
        });

        app.MapPut("/state/segment/move", (
            [FromServices] DataStoreService dataStore,
            [Required] string segmentId,
            string? targetGroupId) =>
        {
            lock (dataStore.lockject)
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
                    targetGroup = LedSegmentGroup.NewGroup;
                    dataStore.Data.Groups.Add(targetGroup);
                }

                segmentGroup.LedSegments.Remove(segment);
                targetGroup.LedSegments.Add(segment);
                if (segmentGroup.LedSegments.Count == 0 && !segmentGroup.IsEdited)
                    dataStore.Data.Groups.Remove(segmentGroup);

                dataStore.Save();
            }

            return Results.Accepted();
        });

        app.MapPut("/state/segment/rename", (
            [FromServices] DataStoreService dataStore,
            [Required] string segmentId,
            [Required] string newName) =>
        {
            lock (dataStore.lockject)
            {
                var segment = dataStore.Data.Groups.SelectMany(x => x.LedSegments).FirstOrDefault(x => x.ReadonlyId == segmentId);
                if (segment == null)
                    return Results.NotFound("The SegmentId was not found in any groups");

                segment.Name = newName;

                dataStore.Save();
            }

            return Results.Accepted();
        });

        app.MapPut("/state/activated", (
            [FromServices] DataStoreService dataStore,
            [Required] bool newActivated) =>
        {
            lock (dataStore.lockject)
            {
                dataStore.Data.Activated = newActivated;

                dataStore.Save();
            }

            return Results.Accepted();
        });
    }
}
