using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Server.Services.DataStore;
using Server.Services.LedTheme;

namespace Server.Endpoints;

public static class RegisterTypeInfoEndpoints
{
    public static void RegisterLedThemeTypeInfoEndpoints(this WebApplication app)
    {
        {
            Type ledThemeType = typeof(LedTheme);
            var attrs = ledThemeType.GetCustomAttributes<JsonDerivedTypeAttribute>();
            foreach (var attr in attrs)
            {
                //attr.DerivedType.;
            }

            app.MapGet("/themes", (
                [FromServices] DataStoreService dataStore) =>
            {
                // TODO: Get from refelction?

                return Results.Accepted(value: attrs);
            });
        }
    }
}
