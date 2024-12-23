namespace Server.Services.DataStore.Types;

public record LedSegment(string WledServerAddress, int SegmentIndex, string? Name = null)
{
    public string ReadonlyId => WledServerAddress + "#" + SegmentIndex;
}