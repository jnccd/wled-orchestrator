using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Server.Services.DataStore;
using Server.Services.DataStore.Types;

namespace Server.Endpoints.Controllers;

[ApiController]
[Route("/state/segments")]
public class SegmentController : ControllerBase
{
    /// <summary>Upper bound for a per-segment gamma override; 0 (or null) means "use the devices own gamma".</summary>
    const double MaxGammaExponentOverride = 10;

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
                .FirstOrDefault(x => x.Item2 != null);
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
            var segment = LedSegment.FindInDatastore(segmentId, dataStore);
            if (segment == null)
                return Results.NotFound("The SegmentId was not found in any groups");

            segment.Name = newName;

            dataStore.Save();
        }

        return Results.Accepted();
    }

    /// <summary>
    /// Sets the per-segment gamma exponent override used when the orchestrator sends this segments
    /// realtime colors over UDP. A value &gt; 0 overrides the devices own gamma for this segment;
    /// 0 (or null) clears the override so the devices reported gamma is used again.
    /// </summary>
    [HttpPut("{segmentId}/gamma")]
    public IResult SetGammaExponentOverride(
        [FromServices] DataStoreService dataStore,
        [Required] string segmentId,
        double gammaExponentOverride)
    {
        if (gammaExponentOverride < 0 || gammaExponentOverride > MaxGammaExponentOverride)
            return Results.BadRequest($"gammaExponentOverride must be between 0 and {MaxGammaExponentOverride}");

        lock (dataStore.lockject)
        {
            var segment = LedSegment.FindInDatastore(segmentId, dataStore);
            if (segment == null)
                return Results.NotFound("The SegmentId was not found in any groups");

            segment.GammaExponentOverride = gammaExponentOverride > 0 ? gammaExponentOverride : null;

            dataStore.Save();
        }

        return Results.Accepted();
    }
}
