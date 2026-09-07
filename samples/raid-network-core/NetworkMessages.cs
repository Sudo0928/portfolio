using FishNet.Broadcast;

namespace ProjectRaid.Network
{
    public struct ShutdownNotice : IBroadcast
    {
        public string Token;
    }

    public struct RequestClientData : IBroadcast
    {
        public string Token;
    }

    public struct DeliveryClientData : IBroadcast
    {
        public string Token;
        public byte[] Payload;
    }
}
