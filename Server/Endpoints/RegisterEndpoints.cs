using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Server.Services.DataStore;

namespace Server.Endpoints;

public static class RegisterEndpoints
{
    public static void RegisterWledEndpoints(this WebApplication app)
    {
        app.MapGet("/state", [ProducesResponseType(typeof(DataStoreRoot), 200)] (
            [FromServices] DataStoreService dataStore, IOptions<JsonOptions> jsonOptions) =>
            Results.Json(dataStore.Data, jsonOptions.Value.JsonSerializerOptions));

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
