using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Server.Services.LedTheme;

namespace Server.Endpoints;

record LedThemeTypes(IEnumerable<TypeInfo> Themes, IEnumerable<TypeInfo> Modifiers);
record TypeInfo(string Name, string? TypeDiscriminator, IEnumerable<TypePropertyInfo> Properties);
record TypePropertyInfo(string Name, string DisplayName, string Type, GenerateFrontendFormData Settings, CompoundTypeInfo? CompoundTypeInfo = null);
record CompoundTypeInfo(string[] ValidEnumValues);
class GenerateFrontendFormData(GenerateFrontendFormAttribute settings)
{
    public string InputType { get; init; } = settings.InputType;
    public double MinValue { get; init; } = settings.MinValue;
    public double MaxValue { get; init; } = settings.MaxValue;
}

[AttributeUsage(AttributeTargets.Property)]
public class GenerateFrontendFormAttribute(string InputType = "", double MinValue = 0, double MaxValue = 100) : Attribute
{
    public readonly string InputType = InputType;
    public readonly double MinValue = MinValue;
    public readonly double MaxValue = MaxValue;
}

public static class RegisterTypeInfoEndpoints
{
    public static void RegisterLedThemeTypeInfoEndpoints(this WebApplication app)
    {
        string ModifyPropertyName(string name)
        {
            var nameBuilder = new StringBuilder(name);
            for (int i = 1; i < nameBuilder.Length; i++)
                if (char.IsUpper(nameBuilder[i]))
                    nameBuilder.Insert(i++, " ");
            return nameBuilder.ToString();
        }

        IEnumerable<TypePropertyInfo> MapProperties(PropertyInfo[] properties, Type parentType) =>
            properties
                .Where(prop => prop.DeclaringType == parentType // Not from parent classes
                        && prop.GetCustomAttribute<GenerateFrontendFormAttribute>() != null)
                    .Select(prop =>
                    {
                        var attrData = new GenerateFrontendFormData(prop.GetCustomAttribute<GenerateFrontendFormAttribute>() ?? new GenerateFrontendFormAttribute());

                        CompoundTypeInfo? validEnumValues = null;
                        if (prop.PropertyType.Name == "EnumSet`1")
                        {
                            validEnumValues = new(Enum.GetNames(prop.PropertyType.GenericTypeArguments.First()));
                        }

                        return new TypePropertyInfo(prop.Name, ModifyPropertyName(prop.Name), prop.PropertyType.Name, attrData, validEnumValues);
                    });

        Type ledThemeType = typeof(LedTheme);
        var ledThemeTypeAttrs = ledThemeType.GetCustomAttributes<JsonDerivedTypeAttribute>();
        var themeTypesInfo = ledThemeTypeAttrs
            .Select(typeAttr => new TypeInfo(
                typeAttr.DerivedType.Name.Replace("LedTheme", ""),
                typeAttr.TypeDiscriminator as string,
                MapProperties(typeAttr.DerivedType.GetProperties(), typeAttr.DerivedType)));

        Type ledThemeModifierType = typeof(LedThemeModifier);
        var ledThemeModifierTypeAttrs = ledThemeModifierType.GetCustomAttributes<JsonDerivedTypeAttribute>();
        var themeModifierTypesInfo = ledThemeModifierTypeAttrs
            .Select(typeAttr => new TypeInfo(
                typeAttr.DerivedType.Name,
                typeAttr.TypeDiscriminator as string,
                MapProperties(typeAttr.DerivedType.GetProperties(), typeAttr.DerivedType)));

        app.MapGet("/theme-types", () => new LedThemeTypes(themeTypesInfo, themeModifierTypesInfo));
    }
}
