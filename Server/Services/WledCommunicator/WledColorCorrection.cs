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
    /// Fallback for servers that do not answer /json/cfg (or return an unparsable payload):
    /// gamma curve 2.8 with realtime correction skipped, the classic defaults of WLED 0.13/0.14/0.15.
    /// </summary>
    public static readonly WledColorCorrection DefaultFallback = new(2.8, deviceAppliesGammaItself: false);

    /// <summary>Gamma value the device applies to non-realtime colors ("light.gc.col"); 1.0 when color gamma is disabled on the device.</summary>
    public double GammaValue { get; }

    /// <summary>True when the device corrects realtime data itself ("if.live.no-gc" == false) so raw colors should be sent.</summary>
    public bool DeviceAppliesGammaItself { get; }

    /// <summary>
    /// Lookup table replicating WLEDs gamma table for <see cref="GammaValue"/> (same curve as
    /// WLEDs colors.cpp calcGammaTable: round(pow(i/255, gamma) * 255)).
    /// Null when no client-side correction is needed.
    /// </summary>
    public byte[]? GammaLut { get; }

    WledColorCorrection(double gammaValue, bool deviceAppliesGammaItself)
    {
        GammaValue = gammaValue;
        DeviceAppliesGammaItself = deviceAppliesGammaItself;

        if (gammaValue > 1.0 && !deviceAppliesGammaItself)
        {
            var lut = new byte[256];
            for (int i = 1; i < 256; i++)
                lut[i] = (byte)(Math.Pow(i / 255.0, gammaValue) * 255.0 + 0.5);
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
