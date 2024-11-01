namespace Server.Services.LedTheme;

public record LedThemeInput(DateTime Time);

// TODO: Polymorphism support for json
public abstract class LedTheme
{
    public readonly List<LedThemeModifier> modifiers = [];

    public abstract LedGroupState? GetNewState(LedThemeInput input);

    public LedGroupState? GetNewModifiedState(LedThemeInput input)
    {
        var state = GetNewState(input);
        foreach (var modifier in modifiers)
            state = modifier.ModifyState(state);
        return state;
    }
}

public abstract class LedThemeModifier
{
    public abstract LedGroupState? ModifyState(LedGroupState? input);
}

