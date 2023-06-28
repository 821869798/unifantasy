
namespace UniFan.Network
{
    public enum ConnectResults
    {
        Success = 1,
        Faild
    }

    public enum SendResults
    {
        Success,
        Pending,
        Ignore,
        Faild
    }


    public enum CloseResults
    {
        Pending,
        Closed,
        InClosing,
    }


    public enum SocketStatus
    {
        Initial = 1,
        Connecting,
        Establish,
        Closing,
        Closed
    }

}
