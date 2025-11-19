using System.Threading;
using System.Threading.Tasks;

namespace DAL.Network
{
    public interface ITcpSender
    {
        Task<TransmissionResult> SendDataViaTcpAsync(
            ProtocolMessage message,
            string objectType,
            string serverIp = "127.0.0.1",
            int port = 6020,
            string protocolVersion = "0.0.1",
            CancellationToken cancellationToken = default);
    }
}