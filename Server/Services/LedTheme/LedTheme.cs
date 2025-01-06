using System.Text.Json.Serialization;
using Server.Services.LedTheme.Themes;

namespace Server.Services.LedTheme;

public record LedThemeInput(DateTime Time);

[JsonDerivedType(typeof(LedThemeDaylight), typeDiscriminator: "themeDaylight")]
[JsonDerivedType(typeof(LedThemeSingleColor), typeDiscriminator: "themeDefault")]
public class LedTheme
{
    public LedTheme() { }

    public string TypeName => GetType().Name;

    public Guid Id { get; set; } = Guid.NewGuid();

    public List<LedThemeModifier> Modifiers { get; } = [];

    public virtual LedGroupState? GetNewState(LedThemeInput input) => null;

    public LedGroupState? GetNewModifiedState(LedThemeInput input)
    {
        var state = GetNewState(input);
        foreach (var modifier in Modifiers)
            state = modifier.ModifyState(state);
        return state;
    }
}

public abstract class LedThemeModifier
{
    public abstract LedGroupState? ModifyState(LedGroupState? input);
}

