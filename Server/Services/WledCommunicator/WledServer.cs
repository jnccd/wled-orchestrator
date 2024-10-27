namespace Server.Services.WledCommunicator;

public class WledServer
{
    public string address;
    public LedState state;

    public WledServer(string address, LedState state)
    {
        this.address = address;
        this.state = state;
    }
}