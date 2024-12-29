namespace Server.Services.DataStore.Types;

public class LedSegment(string WledServerAddress, int SegmentIndex, string? Name = null)
{
    public string WledServerAddress { get; } = WledServerAddress;
    public int SegmentIndex { get; } = SegmentIndex;
    public string? Name { get; set; } = Name;

    public string ReadonlyId => WledServerAddress + "#" + SegmentIndex;

    public override bool Equals(object? obj) =>
        ReadonlyId == (obj as LedSegment)?.ReadonlyId;

    public override int GetHashCode() =>
        ReadonlyId.GetHashCode();
}