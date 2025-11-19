using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Network
{
    public class SimpleTcpClient
    {
        public SimpleTcpClient()
        {
            
        }

        public void SendDataViaTcp(byte [] data, string serverIp, int pornNumber)
        {
            try
            {
                using (TcpClient client = new TcpClient(serverIp, pornNumber))
                using (NetworkStream stream = client.GetStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Fehler bei der Verbindung mit dem Server!:{System.Environment.NewLine}{ex.Message}");
            }
        }
    }
}
