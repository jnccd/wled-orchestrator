using Server.Services.DataStore.Types;

namespace Server.Services.DataStore;

public class DataStoreRoot
{
    public List<LedSegmentGroup> Groups;

    public DataStoreRoot()
    {
        Groups = [new([], null, "Default", new(255, 255, 255))];
    }
}