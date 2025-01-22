using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Server.Services.DataStore;
using Server.Services.LedTheme;

namespace Server.Endpoints.Controllers;

[ApiController]
[Route("/state/group")]
public class GroupController : ControllerBase
{
    [HttpDelete]
    [Route("")]
    public IResult DeleteGroup(
        [FromServices] DataStoreService dataStore,
        [Required] string groupId)
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
    }

    [HttpPut]
    [Route("name")]
    public IResult Rename(
            [FromServices] DataStoreService dataStore,
            [Required] string groupId,
            [Required] string newName)
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
    }

    [HttpPut]
    [Route("theme")]
    public IResult PutTheme(
        [FromServices] DataStoreService dataStore,
        [Required] string groupId,
        [FromBody, Required] LedTheme newTheme)
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
    }
}