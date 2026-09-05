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

    // Byte 1 = number of seconds WLED keeps realtime mode alive after the last received packet.
    // 255 (= 255001 ms) makes WLED stay in live mode indefinitely (until a timeout byte of 0 is
    // received or the device leaves realtime another way). That is what allows the orchestrator to
    // prune duplicate frames: once a picture is on the strip it stays there without further traffic.
    // Whenever the orchestrator stops driving a server it therefore sends an explicit cancel frame
    // (see CancelRealtime), and SendSegmentColors callers are expected to re-send unchanged colors
    // occasionally to recover from devices that left live mode on their own (e.g. after a reboot).
    const byte RealtimeTimeoutSecs = 255;

    // WLED drops datagrams larger than UDP_IN_MAXSIZE (1472). Stay below that so frames are
    // neither dropped by WLED nor IP-fragmented on the wire.
    const int MaxPacketBytes = 1400;

    /// <summary>
    /// Asks the WLED server to leave realtime (live) mode. WLED exits live mode when it receives a
    /// realtime-protocol packet whose timeout byte is 0, regardless of content; a minimal DNRGB
    /// header suffices. Used when the orchestrator stops driving a server so the device can return
    /// to its own effects instead of being stuck showing the last live frame forever.
    /// </summary>
    public static void CancelRealtime(string host, int udpPort = DefaultWledUdpPort)
    {
        if (string.IsNullOrWhiteSpace(host) || udpPort is < 1 or > 65535)
            return;

        byte[] frame = [DnRgbProtocol, 0]; // protocol + timeout 0 = cancel realtime
        Send(frame, host, udpPort);
    }

    /// <summary>
    /// Sends <paramref name="colors"/> to the WLED server at <paramref name="host"/>:<paramref name="udpPort"/>,
    /// starting at LED index <paramref name="startIndex"/>. Long color runs are chunked over multiple
    /// DNRGB frames so each datagram stays under the MTU.
    /// </summary>
    /// <param name="gammaLut">
    /// Optional 256-entry gamma lookup table (see <see cref="WledColorCorrection"/>) applied per channel
    /// before sending. WLED skips its own gamma correction for realtime data by default, so pre-applying
    /// the same curve reproduces the look of WLEDs normal (JSON API) color path. Null sends raw colors.
    /// </param>
    public static void SendSegmentColors(string host, int startIndex, IReadOnlyList<ColorRgb> colors, int udpPort = DefaultWledUdpPort, byte[]? gammaLut = null)
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
                frame[p] = gammaLut == null ? color.R : gammaLut[color.R];
                frame[p + 1] = gammaLut == null ? color.G : gammaLut[color.G];
                frame[p + 2] = gammaLut == null ? color.B : gammaLut[color.B];
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
