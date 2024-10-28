namespace Server.Services.WledCommunicator;

public class WledServer(string address, WledServerState state)
{
    public string address = address;
    public WledServerState state = state;
}