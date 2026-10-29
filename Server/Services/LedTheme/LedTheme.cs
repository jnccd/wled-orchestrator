using System.Text.Json.Serialization;
using Server.Services.LedTheme.Themes;

namespace Server.Services.LedTheme;

public record LedThemeInput(DateTime Time);

public enum LedThemePreviewType
{
    Day, Min
}

[JsonDerivedType(typeof(LedThemeWave), typeDiscriminator: "ledThemeWave")]
[JsonDerivedType(typeof(LedThemeDiscreteWave), typeDiscriminator: "ledThemeDiscreteWave")]
[JsonDerivedType(typeof(LedThemeDaylight), typeDiscriminator: "ledThemeDaylight")]
[JsonDerivedType(typeof(LedThemeSingleColor), typeDiscriminator: "ledThemeDefault")]
public class LedTheme
{
    public LedTheme() { }
    public LedTheme(LedThemePreviewType PreviewType) { this.PreviewType = PreviewType; }

    public Guid Id { get; set; } = Guid.NewGuid();
    public string TypeName => GetType().Name.Replace("LedTheme", "");
    public LedThemePreviewType PreviewType { get; set; } = LedThemePreviewType.Day;

    public List<LedThemeModifier> Modifiers { get; set; } = [];

    public virtual LedGroupState? GetNewState(LedThemeInput input) => null;

    public LedGroupState? GetNewModifiedState(LedThemeInput input)
    {
        var state = GetNewState(input);
        foreach (var modifier in Modifiers)
            state = modifier.ModifyState(state, input);
        return state;
    }

    public LedGroupState? GetPreviewState(int maxIterator, int curIterator)
    {
        var hour = 24 / (float)maxIterator * curIterator;
        var sec = 60 / (float)maxIterator * curIterator;
        // The day preview runs in UTC: the daylight theme derives the sky from the real UTC instant,
        // so a local-time preview would shift the sun arc with the time zone and with DST.
        var utcDay = DateTime.UtcNow.Date;
        var utcHour = new TimeSpan((int)hour, (int)(hour % 1 * 60), 0);
        return PreviewType switch
        {
            LedThemePreviewType.Day => this.GetNewModifiedState(new LedThemeInput(utcDay.Add(utcHour))),
            LedThemePreviewType.Min => this.GetNewModifiedState(new(new(DateOnly.FromDateTime(DateTime.Now), new(0, 0, (int)sec, (int)(sec % 1 * 1000))))),
            _ => null,
        };
    }
}
