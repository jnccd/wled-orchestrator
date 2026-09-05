using Newtonsoft.Json.Linq;

namespace Server.Services.WledCommunicator;

/// <summary>
/// Color-correction info for one WLED server, derived from its /json/cfg response.
///
/// WLED gamma-corrects the colors it renders itself (effects, JSON API writes) using a per-channel
/// gamma lookup table, but by default skips that correction for UDP realtime data (setting
/// "if.live.no-gc" / "gamma correction handled by source"). Because the orchestrator now sends
/// colors over UDP realtime, it re-applies the same gamma curve client-side so the output looks
/// identical to the old JSON per-LED transport.
/// </summary>
public class WledColorCorrection
{
    /// <summary>
    /// STEERING KNOB for the client-side color curve applied to UDP realtime frames.
    ///
    /// The orchestrator sends realtime colors through a gamma lookup table with exponent E so that
    /// the strip reproduces the same curve WLED itself would apply to JSON/effect colors. WLEDs
    /// default is E = 2.8 ("light.gc.col" per device, see <see cref="FromCfgJson"/>).
    ///
    /// Set to 0 to use each device's own reported gamma (current default behaviour).
    /// Set to any value &gt; 0 (e.g. 3.2, 3.5) to override that exponent and steer the brightness
    /// curve yourself:
    ///   - raising the exponent makes low/mid color values darker on the strip (0 and 255 stay fixed),
    ///   - lowering it (e.g. 2.2) makes low/mid values brighter.
    /// A value of 1.0 means no correction (send raw values).
    /// </summary>
    public const double GammaExponentOverride = 3.5;

    /// <summary>
    /// Fallback for servers that do not answer /json/cfg (or return an unparsable payload):
    /// gamma curve 2.8 with realtime correction skipped, the classic defaults of WLED 0.13/0.14/0.15.
    /// </summary>
    public static readonly WledColorCorrection DefaultFallback = new(2.8, deviceAppliesGammaItself: false);

    /// <summary>Gamma value the device applies to non-realtime colors ("light.gc.col"); 1.0 when color gamma is disabled on the device.</summary>
    public double GammaValue { get; }

    /// <summary>Exponent actually used for the client-side lookup table (<see cref="GammaValue"/> or <see cref="GammaExponentOverride"/>).</summary>
    public double EffectiveGammaExponent { get; }

    /// <summary>True when the device corrects realtime data itself ("if.live.no-gc" == false) so raw colors should be sent.</summary>
    public bool DeviceAppliesGammaItself { get; }

    /// <summary>
    /// Lookup table replicating the gamma curve for <see cref="EffectiveGammaExponent"/> (same curve
    /// as WLEDs colors.cpp calcGammaTable: round(pow(i/255, gamma) * 255)).
    /// Null when no client-side correction is needed.
    /// </summary>
    public byte[]? GammaLut { get; }

    WledColorCorrection(double gammaValue, bool deviceAppliesGammaItself)
    {
        GammaValue = gammaValue;
        DeviceAppliesGammaItself = deviceAppliesGammaItself;
        EffectiveGammaExponent = GammaExponentOverride > 0 ? GammaExponentOverride : gammaValue;

        if (EffectiveGammaExponent > 1.0 && !deviceAppliesGammaItself)
        {
            var lut = new byte[256];
            for (int i = 1; i < 256; i++)
                lut[i] = (byte)(Math.Pow(i / 255.0, EffectiveGammaExponent) * 255.0 + 0.5);
            GammaLut = lut;
        }
    }

    /// <summary>
    /// Parses a WLED /json/cfg payload into correction settings. Returns null when the payload is
    /// unusable so the caller can fall back to <see cref="DefaultFallback"/>.
    /// </summary>
    public static WledColorCorrection? FromCfgJson(string? cfgJson)
    {
        if (string.IsNullOrWhiteSpace(cfgJson))
            return null;

        try
        {
            var root = JObject.Parse(cfgJson);

            var gcCol = root["light"]?["gc"]?["col"];
            if (gcCol == null)
                return null; // unexpected payload shape; let the caller use the fallback

            double gamma = gcCol.Value<double>();
            // arlsDisableGammaCorrection; defaults to true on WLED (correction handled by the source)
            bool realtimeSkipsGamma = root["if"]?["live"]?["no-gc"]?.Value<bool>() ?? true;

            if (gamma < 1.0 || gamma > 3.0)
                gamma = 1.0; // WLED sanitizes out-of-range values the same way

            return new WledColorCorrection(gamma, deviceAppliesGammaItself: !realtimeSkipsGamma);
        }
        catch
        {
            return null;
        }
    }
}
