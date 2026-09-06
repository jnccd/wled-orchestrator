using Microsoft.Net.Http.Headers;

namespace Server.Services.DataStore.Types;

public class LedSegment(string WledServerAddress, int SegmentIndex, int Start, int Length, string? Name = null)
{
    public string WledServerAddress { get; } = WledServerAddress;
    public int SegmentIndex { get; } = SegmentIndex;
    public string? Name { get; set; } = Name;

    public int Start { get; set; } = Start;
    public int Length { get; set; } = Length;

    /// <summary>
    /// Optional per-segment client-side gamma exponent, applied when the orchestrator sends realtime
    /// colors over UDP. Null (or 0) means "use this devices own reported gamma"
    /// (see <see cref="WledColorCorrection.EffectiveGammaExponent(double?)"/>). When set to a value
    /// &gt; 0 it steers the brightness curve of this segment independently of the devices gamma.
    /// </summary>
    public double? GammaExponentOverride { get; set; }

    /// <summary>
    /// The gamma exponent the devices reports for non-realtime colors ("light.gc.col"). Populated by
    /// the communicator when segments are discovered and used as the baseline reference value in the
    /// UI so a segment with no override can be shown/edited against a concrete number.
    /// </summary>
    public double? DeviceGammaExponent { get; set; }

    public string Id => WledServerAddress.Split('/').Last() + "-" + SegmentIndex;

    public static LedSegment? FindInDatastore(string segmentId, DataStoreService dataStore) =>
        dataStore.Data.Groups.SelectMany(x => x.LedSegments).FirstOrDefault(x => x.Id == segmentId);

    public override bool Equals(object? obj) =>
        Id == (obj as LedSegment)?.Id;

    public override int GetHashCode() =>
        Id.GetHashCode();
}