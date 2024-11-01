using Server.Services.DataStore.Types;

namespace Server.Services.DataStore;

public class DataStoreRoot
{
    public List<LedSegmentGroup> Groups;

    public DataStoreRoot()
    {
        Groups = [LedSegmentGroup.DefaultGroup];
    }
}