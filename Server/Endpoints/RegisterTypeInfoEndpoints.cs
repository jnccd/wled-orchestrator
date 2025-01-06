using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Server.Services.DataStore;
using Server.Services.LedTheme;

namespace Server.Endpoints;

record LedThemeTypes(IEnumerable<TypeInfo> themes, IEnumerable<TypeInfo> modifiers);
record TypeInfo(string Name, string? TypeDiscriminator, IEnumerable<TypePropertyInfo> Properties);
record TypePropertyInfo(string Name, string Type, object? defaultValue = null);

public static class RegisterTypeInfoEndpoints
{
    public static void RegisterLedThemeTypeInfoEndpoints(this WebApplication app)
    {
        Type ledThemeType = typeof(LedTheme);
        var ledThemeTypeAttrs = ledThemeType.GetCustomAttributes<JsonDerivedTypeAttribute>();
        var themeTypesInfo = ledThemeTypeAttrs
            .Select(x => new TypeInfo(
                x.DerivedType.Name,
                x.TypeDiscriminator as string,
                x.DerivedType.GetProperties()
                    .Where(y => y.DeclaringType == x.DerivedType)
                    .Select(y => new TypePropertyInfo(y.Name, y.PropertyType.Name, y.GetCustomAttribute<DefaultValueAttribute>()?.Value))));

        Type ledThemeModifierType = typeof(LedThemeModifier);
        var ledThemeModifierTypeAttrs = ledThemeModifierType.GetCustomAttributes<JsonDerivedTypeAttribute>();
        var themeModifierTypesInfo = ledThemeModifierTypeAttrs
            .Select(x => new TypeInfo(
                x.DerivedType.Name,
                x.TypeDiscriminator as string,
                x.DerivedType.GetProperties()
                    .Where(y => y.DeclaringType == x.DerivedType)
                    .Select(y => new TypePropertyInfo(y.Name, y.PropertyType.Name, y.GetCustomAttribute<DefaultValueAttribute>()?.Value))));

        app.MapGet("/theme-types", () => new LedThemeTypes(themeTypesInfo, themeModifierTypesInfo));
    }
}
