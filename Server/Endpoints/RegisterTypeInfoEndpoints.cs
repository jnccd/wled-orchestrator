using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Server.Services.DataStore;
using Server.Services.LedTheme;

namespace Server.Endpoints;

record LedThemeTypes(IEnumerable<LedThemeImpl> themes, IEnumerable<LedThemeModifierImpl> modifiers);
record LedThemeImpl(string Name, IEnumerable<LedThemeImplProperty> Properties);
record LedThemeImplProperty(string Name, string Type);
record LedThemeModifierImpl(string Name, IEnumerable<LedThemeModifierImplProperty> Properties);
record LedThemeModifierImplProperty(string Name, string Type);

public static class RegisterTypeInfoEndpoints
{
    public static void RegisterLedThemeTypeInfoEndpoints(this WebApplication app)
    {
        Type ledThemeType = typeof(LedTheme);
        var ledThemeTypeAttrs = ledThemeType.GetCustomAttributes<JsonDerivedTypeAttribute>();
        var themeTypesInfo = ledThemeTypeAttrs
            .Select(x => new LedThemeImpl(
                x.TypeDiscriminator as string ?? "Not Found",
                x.DerivedType.GetProperties()
                    .Where(y => y.DeclaringType == x.DerivedType)
                    .Select(y => new LedThemeImplProperty(y.Name, y.PropertyType.Name))));

        Type ledThemeModifierType = typeof(LedThemeModifier);
        var ledThemeModifierTypeAttrs = ledThemeModifierType.GetCustomAttributes<JsonDerivedTypeAttribute>();
        var themeModifierTypesInfo = ledThemeModifierTypeAttrs
            .Select(x => new LedThemeModifierImpl(
                x.TypeDiscriminator as string ?? "Not Found",
                x.DerivedType.GetProperties()
                    .Where(y => y.DeclaringType == x.DerivedType)
                    .Select(y => new LedThemeModifierImplProperty(y.Name, y.PropertyType.Name))));

        app.MapGet("/themes", () => new LedThemeTypes(themeTypesInfo, themeModifierTypesInfo));
    }
}
