using Server.Services.DataStore.Types;

namespace Server.Services.DataStore;

public class DataStoreRoot()
{
    public List<LedSegment> Segments = [];
    public List<LedSegmentGroup> Groups = [];
}