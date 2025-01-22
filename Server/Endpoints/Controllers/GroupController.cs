using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Server.Services.DataStore;
using Server.Services.LedTheme;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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
                return Results.NotFound("The GroupId was not found in any groups!");

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
                return Results.NotFound("The GroupId was not found in any groups!");

            group.Theme = newTheme;

            dataStore.Save();
        }

        return Results.Accepted();
    }

    [HttpGet("theme-preview")]
    [ProducesResponseType(typeof(IResult), 200, "image/png")]
    public IResult GetThemePreview(
        [FromServices] DataStoreService dataStore,
        [Required] string groupId)
    {
        Image<Rgba32> image;

        lock (dataStore.lockject)
        {
            var group = dataStore.Data.Groups.FirstOrDefault(x => x.Id == Guid.Parse(groupId));
            if (group == null)
                return Results.NotFound("The GroupId was not found in any groups!");

            if (group.Theme == null)
                return Results.NotFound("The group does not have a theme!");

            var testState = group.Theme.GetNewState(new(new(2000, 1, 1, 0, 0, 0)));

            if (testState == null)
                return Results.NotFound("Cant get state from theme!");

            image = new(testState.Colors.Length, testState.Colors.Length * 16 / 9);
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgba32> pixelRow = accessor.GetRowSpan(y);

                    var hour = 24 / (float)accessor.Height * y;
                    var state = group.Theme.GetNewState(new(new(2000, 1, 1, (int)hour, (int)(hour % 1 * 60), 0)));

                    for (int x = 0; x < pixelRow.Length; x++)
                    {
                        ref Rgba32 pixel = ref pixelRow[x];
                        pixel.R = state!.Colors[x].R;
                        pixel.G = state!.Colors[x].G;
                        pixel.B = state!.Colors[x].B;
                        pixel.A = 255;
                    }
                }
            });
        }

        Stream imageStream = new MemoryStream(image.Width * image.Height * image.PixelType.BitsPerPixel / 8);
        image.SaveAsPng(imageStream);
        imageStream.Position = 0;

        return Results.File(imageStream, "image/png", "theme-preview");
    }
}