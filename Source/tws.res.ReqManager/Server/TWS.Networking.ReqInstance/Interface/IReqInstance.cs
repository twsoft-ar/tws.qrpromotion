using System.Net.Sockets;

namespace TWS.Networking
{
    public interface IReqInstance
    {
        void StartProcess();
        void StartProcess(Socket socket_);
    }
}