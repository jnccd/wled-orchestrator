using Server.Helper;

namespace Server.Services.DataStore.Types;

public class LedSegmentGroup(string Name, List<LedSegment> LedSegments, LedTheme.LedTheme? Theme, ColorRgb? DisplayColor)
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = Name;
    public List<LedSegment> LedSegments { get; } = LedSegments ?? [];
    public LedTheme.LedTheme? Theme { get; set; } = Theme;
    public ColorRgb? DisplayColor { get; set; } = DisplayColor;

    public bool IsEdited => DisplayColor != null || Theme != null;

    public static LedSegmentGroup DefaultGroup => new("Default", [], null, null);
    public static LedSegmentGroup NewGroup => new("New Group", [], null, null);
}