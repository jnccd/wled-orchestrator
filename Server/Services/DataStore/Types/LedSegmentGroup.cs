using Server.Helper;

namespace Server.Services.DataStore.Types;

public class LedSegmentGroup(string Name, List<LedSegment> LedSegments, LedTheme.LedTheme? Theme, ColorRgb? DisplayColor)
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = Name;
    public List<LedSegment> LedSegments { get; } = LedSegments ?? [];
    public LedTheme.LedTheme? Theme { get; set; } = Theme;

    // Last settings this group used for each theme type, keyed by the JSON type discriminator
    // (e.g. "ledThemeDaylight"). Picking a type again in the editor restores them instead of
    // resetting every parameter to its default.
    public Dictionary<string, LedTheme.LedTheme> ThemeStash { get; set; } = [];

    public ColorRgb? DisplayColor { get; set; } = DisplayColor;

    public bool IsEdited => DisplayColor != null || Theme != null;

    public static LedSegmentGroup DefaultGroup => new("Default", [], null, null);
    public static LedSegmentGroup NewGroup => new("New Group", [], null, null);
}