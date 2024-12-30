using Server.Services.DataStore.Types;

namespace Server.Services.DataStore;

public class DataStoreRoot
{
    public bool Activated { get; set; }
    public List<LedSegmentGroup> Groups { get; set; }

    public DataStoreRoot()
    {
        Groups = [];
    }
}