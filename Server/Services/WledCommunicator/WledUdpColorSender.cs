using System.Net.Sockets;
using Server.Helper;

namespace Server.Services.WledCommunicator;

/// <summary>
/// Sends raw pixel colors to WLED servers using WLEDs "UDP realtime" protocol (DNRGB mode).
/// This is the same UDP channel WLEDs sync notifier uses, so no extra WLED configuration is
/// required while the server runs its default UDP port (see <see cref="DefaultWledUdpPort"/>).
/// Docs: https://kno.wled.ge/interfaces/udp-realtime/
/// </summary>
public static class WledUdpColorSender
{
    /// <summary>
    /// Default UDP port WLED listens on for sync notifications &amp; realtime packets.
    /// It is configurable per server in WLED (Settings -&gt; Sync -&gt; UDP Port), so this default is
    /// assumed unless a different port is passed in (per-server custom ports can come later via the UI).
    /// </summary>
    public const int DefaultWledUdpPort = 21324;

    // WLED "UDP realtime" byte 0 selects the protocol:
    // 1 = WARLS, 2 = DRGB, 3 = DRGBW, 4 = DNRGB, 5 = DNRGBW
    const byte DnRgbProtocol = 4;

    // Seconds WLED keeps realtime mode alive after the last received packet before returning to
    // its regular mode. The update loop runs every 500ms, so 2s leaves enough slack for a
    // hiccup without the strip flickering back to WLEDs own state between frames.
    const byte RealtimeTimeoutSecs = 2;

    // WLED drops datagrams larger than UDP_IN_MAXSIZE (1472). Stay below that so frames are
    // neither dropped by WLED nor IP-fragmented on the wire.
    const int MaxPacketBytes = 1400;

    /// <summary>
    /// Sends <paramref name="colors"/> to the WLED server at <paramref name="host"/>:<paramref name="udpPort"/>,
    /// starting at LED index <paramref name="startIndex"/>. Long color runs are chunked over multiple
    /// DNRGB frames so each datagram stays under the MTU.
    /// </summary>
    public static void SendSegmentColors(string host, int startIndex, IReadOnlyList<ColorRgb> colors, int udpPort = DefaultWledUdpPort)
    {
        if (string.IsNullOrWhiteSpace(host) || colors == null || colors.Count == 0 || udpPort is < 1 or > 65535)
            return;

        const int headerSize = 4; // protocol, timeout seconds, start index (high, low)
        int maxLedsPerPacket = (MaxPacketBytes - headerSize) / 3;

        for (int offset = 0; offset < colors.Count; offset += maxLedsPerPacket)
        {
            int count = Math.Min(maxLedsPerPacket, colors.Count - offset);
            var frame = new byte[headerSize + count * 3];

            int startLed = startIndex + offset;
            frame[0] = DnRgbProtocol;
            frame[1] = RealtimeTimeoutSecs;
            frame[2] = (byte)(startLed >> 8);
            frame[3] = (byte)startLed;

            for (int i = 0; i < count; i++)
            {
                var color = colors[offset + i];
                int p = headerSize + i * 3;
                frame[p] = color.R;
                frame[p + 1] = color.G;
                frame[p + 2] = color.B;
            }

            Send(frame, host, udpPort);
        }
    }

    static void Send(byte[] datagram, string host, int port)
    {
        using var udpClient = new UdpClient();
        udpClient.Send(datagram, datagram.Length, host, port);
    }
}
