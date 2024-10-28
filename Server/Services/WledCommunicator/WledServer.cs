namespace Server.Services.WledCommunicator;

public class WledServer(string address, LedState state)
{
    public string address = address;
    public LedState state = state;
}