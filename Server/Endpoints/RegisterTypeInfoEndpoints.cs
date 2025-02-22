using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Server.Services.LedTheme;

namespace Server.Endpoints;

record LedThemeTypes(IEnumerable<TypeInfo> Themes, IEnumerable<TypeInfo> Modifiers);
record TypeInfo(string Name, string? TypeDiscriminator, IEnumerable<TypePropertyInfo> Properties);
record TypePropertyInfo(string Name, string DisplayName, string Type, GenerateFrontendFormData Settings);
class GenerateFrontendFormData(GenerateFrontendFormAttribute settings)
{
    public readonly double MinValue = settings.MinValue;
    public readonly double MaxValue = settings.MaxValue;
}

[AttributeUsage(AttributeTargets.Property)]
public class GenerateFrontendFormAttribute(double MinValue = 0, double MaxValue = 150) : Attribute
{
    public readonly double MinValue = MinValue;
    public readonly double MaxValue = MaxValue;
}

public static class RegisterTypeInfoEndpoints
{
    public static void RegisterLedThemeTypeInfoEndpoints(this WebApplication app)
    {
        string propertyNameModifier(string name)
        {
            var nameBuilder = new StringBuilder(name);
            for (int i = 1; i < nameBuilder.Length; i++)
                if (char.IsUpper(nameBuilder[i]))
                    nameBuilder.Insert(i++, " ");
            return nameBuilder.ToString();
        }

        Type ledThemeType = typeof(LedTheme);
        var ledThemeTypeAttrs = ledThemeType.GetCustomAttributes<JsonDerivedTypeAttribute>();
        var themeTypesInfo = ledThemeTypeAttrs
            .Select(typeAttr => new TypeInfo(
                typeAttr.DerivedType.Name.Replace("LedTheme", ""),
                typeAttr.TypeDiscriminator as string,
                typeAttr.DerivedType.GetProperties()
                    .Where(prop => prop.DeclaringType == typeAttr.DerivedType // Not from parent classes
                        && prop.GetCustomAttribute<GenerateFrontendFormAttribute>() != null)
                    .Select(prop => new TypePropertyInfo(prop.Name, propertyNameModifier(prop.Name), prop.PropertyType.Name, new GenerateFrontendFormData(prop.GetCustomAttribute<GenerateFrontendFormAttribute>() ?? new GenerateFrontendFormAttribute())))));

        Type ledThemeModifierType = typeof(LedThemeModifier);
        var ledThemeModifierTypeAttrs = ledThemeModifierType.GetCustomAttributes<JsonDerivedTypeAttribute>();
        var themeModifierTypesInfo = ledThemeModifierTypeAttrs
            .Select(typeAttr => new TypeInfo(
                typeAttr.DerivedType.Name,
                typeAttr.TypeDiscriminator as string,
                typeAttr.DerivedType.GetProperties()
                    .Where(prop => prop.DeclaringType == typeAttr.DerivedType // Not from parent classes
                        && prop.GetCustomAttribute<GenerateFrontendFormAttribute>() != null)
                    .Select(prop => new TypePropertyInfo(prop.Name, propertyNameModifier(prop.Name), prop.PropertyType.Name, new GenerateFrontendFormData(prop.GetCustomAttribute<GenerateFrontendFormAttribute>() ?? new GenerateFrontendFormAttribute())))));

        app.MapGet("/theme-types", () => new LedThemeTypes(themeTypesInfo, themeModifierTypesInfo));
    }
}
