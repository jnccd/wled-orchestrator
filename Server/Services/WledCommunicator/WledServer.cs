using Server.Helper;
using Server.Services.DataStore.Types;

namespace Server.Services.WledCommunicator;

public record WledServer(string Address, WledServerState State)
{
    public LedSegment[] Segments
    {
        get
        {
            return State.Seg.WithIndex().Select((x, i) => new LedSegment(Address, i)).ToArray();
        }
    }
}