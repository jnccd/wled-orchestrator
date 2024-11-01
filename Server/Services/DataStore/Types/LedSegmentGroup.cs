using Server.Helper;

namespace Server.Services.DataStore.Types;

public record LedSegmentGroup(List<LedSegment> LedSegments, LedTheme.LedTheme? Theme, string Name, Color Color)
{
    public static LedSegmentGroup DefaultGroup { get => new([], null, "Default", new(255, 255, 255)); }
}