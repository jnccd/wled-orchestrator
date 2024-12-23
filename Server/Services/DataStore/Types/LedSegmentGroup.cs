using Server.Helper;

namespace Server.Services.DataStore.Types;

public class LedSegmentGroup(string Name, List<LedSegment> LedSegments, LedTheme.LedTheme? Theme, Color DisplayColor)
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; } = Name;
    public List<LedSegment> LedSegments { get; } = LedSegments;
    public LedTheme.LedTheme? Theme { get; set; } = Theme;
    public Color DisplayColor { get; set; } = DisplayColor;

    public static LedSegmentGroup DefaultGroup { get => new("Default", [], null, new(255, 255, 255)); }
}