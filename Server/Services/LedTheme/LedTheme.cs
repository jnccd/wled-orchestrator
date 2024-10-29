namespace Server.Services.LedTheme;

public record LedThemeInput(DateTime Time);

public abstract class LedTheme
{
    public readonly List<LedThemeModifier> modifiers = [];

    public abstract LedSegmentState? GetNewState(LedThemeInput input);

    public LedSegmentState? GetNewModifiedState(LedThemeInput input)
    {
        var state = GetNewState(input);
        foreach (var modifier in modifiers)
            state = modifier.ModifyState(state);
        return state;
    }
}

public abstract class LedThemeModifier
{
    public abstract LedSegmentState? ModifyState(LedSegmentState? input);
}

