using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Server.Services.DataStore;

namespace Server.Endpoints;

public static class RegisterEndpoints
{
    public static void RegisterWledEndpoints(this WebApplication app)
    {
        app.MapGet("/state", [ProducesResponseType(typeof(DataStoreRoot), 200)] (
            [FromServices] DataStoreService dataStore) =>
            Results.Json(dataStore.Data));

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
