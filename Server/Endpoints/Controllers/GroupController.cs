using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Server.Helper;
using Server.Services.DataStore;
using Server.Services.LedTheme;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace Server.Endpoints.Controllers;

[ApiController]
[Route("/state/groups")]
public class GroupController : ControllerBase
{
    [HttpDelete("{groupId}")]
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

    [HttpPut("{groupId}/name")]
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

    [HttpPut("{groupId}/theme")]
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

    [HttpGet("{groupId}/theme-preview")]
    [ProducesResponseType(typeof(string), 200, "image/png")]
    public IActionResult GetThemePreview(
        [FromServices] DataStoreService dataStore,
        [Required] string groupId)
    {
        Image<Rgba32> image;

        lock (dataStore.lockject)
        {
            if (!Guid.TryParse(groupId, out Guid groupGuid))
                return BadRequest("Wrong Guid!");

            var group = dataStore.Data.Groups.FirstOrDefault(x => x.Id == groupGuid);
            if (group == null)
                return NotFound("The GroupId was not found in any groups!");

            if (group.Theme == null)
                return NotFound("The group does not have a theme!");

            var testState = group.Theme.GetNewState(new(new(2000, 1, 1, 0, 0, 0)));

            if (testState == null)
                return NotFound("Cant get state from theme!");

            image = new(testState.Colors.Length, testState.Colors.Length * 16 / 9);
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgba32> pixelRow = accessor.GetRowSpan(y);

                    var hour = 24 / (float)accessor.Height * y;
                    var state = group.Theme.GetNewModifiedState(new(new(2000, 1, 1, (int)hour, (int)(hour % 1 * 60), 0)));
                    var colors = state!.Colors.Select(x => x.HsvToRgb()).ToArray();

                    for (int x = 0; x < pixelRow.Length; x++)
                    {
                        ref Rgba32 pixel = ref pixelRow[x];
                        pixel.R = colors[x].R;
                        pixel.G = colors[x].G;
                        pixel.B = colors[x].B;
                        pixel.A = 255;
                    }
                }
            });
        }

        return Ok(image.ToBase64String(PngFormat.Instance));
    }

    [HttpPost("{groupId}/theme/modifiers")]
    public IResult PostThemeModifier(
        [FromServices] DataStoreService dataStore,
        [Required] string groupId,
        [Required] LedThemeModifier newModifier,
        int? index)
    {
        lock (dataStore.lockject)
        {
            var group = dataStore.Data.Groups.FirstOrDefault(x => x.Id == Guid.Parse(groupId));
            if (group == null)
                return Results.NotFound("The GroupId was not found in any groups!");
            if (group.Theme == null)
                return Results.NotFound("The groups theme is not set!");

            if (index == null)
                group.Theme.Modifiers.Add(newModifier);
            else
                group.Theme.Modifiers.Insert(Math.Clamp(index.Value, 0, group.Theme.Modifiers.Count), newModifier);

            dataStore.Save();
        }

        return Results.Accepted();
    }

    [HttpPost("{groupId}/theme/modifiers/{modifierId}/copy")]
    public IResult CopyThemeModifier(
        [FromServices] DataStoreService dataStore,
        [Required] string groupId,
        [Required] string modifierId)
    {
        lock (dataStore.lockject)
        {
            var group = dataStore.Data.Groups.FirstOrDefault(x => x.Id == Guid.Parse(groupId));
            if (group == null)
                return Results.NotFound("The GroupId was not found in any groups!");
            if (group.Theme == null)
                return Results.NotFound("The groups theme is not set!");

            var (modifier, modifierIndex) = group.Theme.Modifiers.WithIndex().FirstOrDefault(x => x.item.Id == Guid.Parse(modifierId));
            if (modifier == null)
                return Results.NotFound("The ModifierId was not found in the modifier list!");

            group.Theme.Modifiers.Insert(modifierIndex, (LedThemeModifier)modifier.Clone());

            dataStore.Save();
        }

        return Results.Accepted();
    }

    [HttpPut("{groupId}/theme/modifiers/{modifierId}")]
    public IResult PutThemeModifier(
        [FromServices] DataStoreService dataStore,
        [Required] string groupId,
        [Required] string modifierId,
        [Required] LedThemeModifier newModifier)
    {
        lock (dataStore.lockject)
        {
            var group = dataStore.Data.Groups.FirstOrDefault(x => x.Id == Guid.Parse(groupId));
            if (group == null)
                return Results.NotFound("The GroupId was not found in any groups!");
            if (group.Theme == null)
                return Results.NotFound("The groups theme is not set!");

            var (modifier, modifierIndex) = group.Theme.Modifiers.WithIndex().FirstOrDefault(x => x.item.Id == Guid.Parse(modifierId));
            if (modifier == null)
                return Results.NotFound("The ModifierId was not found in the modifier list!");

            group.Theme.Modifiers[modifierIndex] = newModifier;

            dataStore.Save();
        }

        return Results.Accepted();
    }

    [HttpDelete("{groupId}/theme/modifiers/{modifierId}")]
    public IResult DeleteThemeModifier(
        [FromServices] DataStoreService dataStore,
        [Required] string groupId,
        [Required] string modifierId)
    {
        lock (dataStore.lockject)
        {
            var group = dataStore.Data.Groups.FirstOrDefault(x => x.Id == Guid.Parse(groupId));
            if (group == null)
                return Results.NotFound("The GroupId was not found in any groups!");
            if (group.Theme == null)
                return Results.NotFound("The groups theme is not set!");

            var (modifier, modifierIndex) = group.Theme.Modifiers.WithIndex().FirstOrDefault(x => x.item.Id == Guid.Parse(modifierId));
            if (modifier == null)
                return Results.NotFound("The ModifierId was not found in the modifier list!");

            group.Theme.Modifiers.RemoveAt(modifierIndex);

            dataStore.Save();
        }

        return Results.Accepted();
    }
}