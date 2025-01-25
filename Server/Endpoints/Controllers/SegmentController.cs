using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Server.Services.DataStore;
using Server.Services.DataStore.Types;

namespace Server.Endpoints.Controllers;

[ApiController]
[Route("/state/segments")]
public class SegmentController : ControllerBase
{
    [HttpPut("{segmentId}/move")]
    public IResult Move(
        [FromServices] DataStoreService dataStore,
        [Required] string segmentId,
        string? targetGroupId)
    {
        lock (dataStore.lockject)
        {
            (var segmentGroup, var segment) = dataStore.Data.Groups
                .Select(g => (g, g.LedSegments.FirstOrDefault(x => x.Id == segmentId)))
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
    }

    [HttpPut("{segmentId}/name")]
    public IResult Rename(
        [FromServices] DataStoreService dataStore,
        [Required] string segmentId,
        [Required] string newName)
    {
        lock (dataStore.lockject)
        {
            var segment = dataStore.Data.Groups.SelectMany(x => x.LedSegments).FirstOrDefault(x => x.Id == segmentId);
            if (segment == null)
                return Results.NotFound("The SegmentId was not found in any groups");

            segment.Name = newName;

            dataStore.Save();
        }

        return Results.Accepted();
    }
}
