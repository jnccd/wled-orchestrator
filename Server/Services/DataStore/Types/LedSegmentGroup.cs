using Server.Helper;

namespace Server.Services.DataStore.Types;

public record LedSegmentGroup(string Name, List<LedSegment> LedSegments, LedTheme.LedTheme? Theme, Color DisplayColor)
{
    public static LedSegmentGroup DefaultGroup { get => new("Default", [], null, new(255, 255, 255)); }
}