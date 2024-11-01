using Server.Helper;

namespace Server.Services.DataStore.Types;

public record LedSegmentGroup(List<LedSegment> LedSegments, LedTheme.LedTheme? Theme, string Name, Color Color);