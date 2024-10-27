using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Server.WledCommunicator;

public partial class LedState
{
    [JsonProperty("on", NullValueHandling = NullValueHandling.Ignore)]
    public bool? On { get; set; }

    [JsonProperty("bri", NullValueHandling = NullValueHandling.Ignore)]
    public long? Bri { get; set; }

    [JsonProperty("transition", NullValueHandling = NullValueHandling.Ignore)]
    public long? Transition { get; set; }

    [JsonProperty("ps", NullValueHandling = NullValueHandling.Ignore)]
    public long? Ps { get; set; }

    [JsonProperty("pl", NullValueHandling = NullValueHandling.Ignore)]
    public long? Pl { get; set; }

    [JsonProperty("nl", NullValueHandling = NullValueHandling.Ignore)]
    public Nl Nl { get; set; }

    [JsonProperty("udpn", NullValueHandling = NullValueHandling.Ignore)]
    public Udpn Udpn { get; set; }

    [JsonProperty("lor", NullValueHandling = NullValueHandling.Ignore)]
    public long? Lor { get; set; }

    [JsonProperty("mainseg", NullValueHandling = NullValueHandling.Ignore)]
    public long? Mainseg { get; set; }

    [JsonProperty("seg", NullValueHandling = NullValueHandling.Ignore)]
    public Seg[] Seg { get; set; }
}

public partial class Nl
{
    [JsonProperty("on", NullValueHandling = NullValueHandling.Ignore)]
    public bool? On { get; set; }

    [JsonProperty("dur", NullValueHandling = NullValueHandling.Ignore)]
    public long? Dur { get; set; }

    [JsonProperty("mode", NullValueHandling = NullValueHandling.Ignore)]
    public long? Mode { get; set; }

    [JsonProperty("tbri", NullValueHandling = NullValueHandling.Ignore)]
    public long? Tbri { get; set; }

    [JsonProperty("rem", NullValueHandling = NullValueHandling.Ignore)]
    public long? Rem { get; set; }
}

public partial class Seg
{
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public long? Id { get; set; }

    [JsonProperty("start", NullValueHandling = NullValueHandling.Ignore)]
    public long? Start { get; set; }

    [JsonProperty("stop", NullValueHandling = NullValueHandling.Ignore)]
    public long? Stop { get; set; }

    [JsonProperty("len", NullValueHandling = NullValueHandling.Ignore)]
    public long? Len { get; set; }

    [JsonProperty("grp", NullValueHandling = NullValueHandling.Ignore)]
    public long? Grp { get; set; }

    [JsonProperty("spc", NullValueHandling = NullValueHandling.Ignore)]
    public long? Spc { get; set; }

    [JsonProperty("of", NullValueHandling = NullValueHandling.Ignore)]
    public long? Of { get; set; }

    [JsonProperty("on", NullValueHandling = NullValueHandling.Ignore)]
    public bool? On { get; set; }

    [JsonProperty("frz", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Frz { get; set; }

    [JsonProperty("bri", NullValueHandling = NullValueHandling.Ignore)]
    public long? Bri { get; set; }

    [JsonProperty("cct", NullValueHandling = NullValueHandling.Ignore)]
    public long? Cct { get; set; }

    [JsonProperty("col", NullValueHandling = NullValueHandling.Ignore)]
    public long[][] Col { get; set; }

    [JsonProperty("fx", NullValueHandling = NullValueHandling.Ignore)]
    public long? Fx { get; set; }

    [JsonProperty("sx", NullValueHandling = NullValueHandling.Ignore)]
    public long? Sx { get; set; }

    [JsonProperty("ix", NullValueHandling = NullValueHandling.Ignore)]
    public long? Ix { get; set; }

    [JsonProperty("pal", NullValueHandling = NullValueHandling.Ignore)]
    public long? Pal { get; set; }

    [JsonProperty("sel", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Sel { get; set; }

    [JsonProperty("rev", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Rev { get; set; }

    [JsonProperty("mi", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Mi { get; set; }
}

public partial class Udpn
{
    [JsonProperty("send", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Send { get; set; }

    [JsonProperty("recv", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Recv { get; set; }
}

public partial class LedState
{
    public static LedState FromJson(string json) => JsonConvert.DeserializeObject<LedState>(json, Converter.Settings);
}

public static class Serialize
{
    public static string ToJson(this LedState self) => JsonConvert.SerializeObject(self, Converter.Settings);
}

internal static class Converter
{
    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        DateParseHandling = DateParseHandling.None,
        Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
    };
}
