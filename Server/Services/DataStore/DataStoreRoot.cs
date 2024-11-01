using Server.Services.DataStore.Types;

namespace Server.Services.DataStore;

public class DataStoreRoot
{
    public List<LedSegmentGroup> Groups { get; set; }

    public DataStoreRoot()
    {
        Groups = [];
    }
}