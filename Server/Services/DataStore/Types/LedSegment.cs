namespace Server.Services.DataStore.Types;

public class LedSegment(string WledServerAddress, int SegmentIndex, string? Name = null)
{
    public string WledServerAddress { get; } = WledServerAddress;
    public int SegmentIndex { get; } = SegmentIndex;
    public string? Name { get; set; } = Name;

    public string Id => WledServerAddress.Split('/').Last() + "-" + SegmentIndex;

    public override bool Equals(object? obj) =>
        Id == (obj as LedSegment)?.Id;

    public override int GetHashCode() =>
        Id.GetHashCode();
}