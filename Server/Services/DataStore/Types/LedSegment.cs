using Microsoft.Net.Http.Headers;

namespace Server.Services.DataStore.Types;

public class LedSegment(string WledServerAddress, int SegmentIndex, int Start, int Length, string? Name = null)
{
    public string WledServerAddress { get; } = WledServerAddress;
    public int SegmentIndex { get; } = SegmentIndex;
    public string? Name { get; set; } = Name;

    public int Start { get; set; } = Start;
    public int Length { get; set; } = Length;

    public string Id => WledServerAddress.Split('/').Last() + "-" + SegmentIndex;

    public static LedSegment? FindInDatastore(string segmentId, DataStoreService dataStore) =>
        dataStore.Data.Groups.SelectMany(x => x.LedSegments).FirstOrDefault(x => x.Id == segmentId);

    public override bool Equals(object? obj) =>
        Id == (obj as LedSegment)?.Id;

    public override int GetHashCode() =>
        Id.GetHashCode();
}