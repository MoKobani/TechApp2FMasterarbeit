using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DAL.Network
{
    public sealed class TcpSender
    {
        private const byte SOH = ProtocolMessage.SOH;
        private const byte EOT = ProtocolMessage.EOT;
        private const byte CAN = ProtocolMessage.CAN;

        public async Task<TransmissionResult> SendDataViaTcpAsync(
            ProtocolMessage message,
            string dbDescriptor,              // z.B. "2:1"
            string serverIp,
            int port,
            string expectedVersion,           // z.B. "0.0.1"
            CancellationToken ct)
        {
            using var client = new TcpClient();
            await client.ConnectAsync(serverIp, port);
            using var stream = client.GetStream();

            // 1) Server-Version lesen: <<1>>VERSION<<4>>
            string version = await ReadFrameAsStringAsync(stream, ct, expectSoh: true);
            if (!string.Equals(version, expectedVersion, StringComparison.OrdinalIgnoreCase))
                return TransmissionResult.Fail($"Inkompatible Version: '{version}' (erwartet {expectedVersion})");

            // 3) DB-Descriptor senden: <<1>>"2:1"<<4>>
            await WriteFrameAsync(stream, dbDescriptor, ct);

            // 3) OK/CAN lesen (ein Byte; bei CAN folgt Fehltext bis <<4>>)
            int b = stream.ReadByte();
            if (b == -1) return TransmissionResult.Fail("Verbindung vom Server geschlossen.");
            if (b == CAN)
            {
                string err = await ReadUntilAsync(stream, EOT, ct);
                return TransmissionResult.Fail($"DB abgelehnt: {err}");
            }
            // bei OK (<<1>>) geht's weiter

            // 4) Nutzdaten senden (SOH + payload + EOT)
            var framed = message.ToFramedUtf8();
            await stream.WriteAsync(framed, 0, framed.Length, ct);
            await stream.FlushAsync(ct);

            // 5) Abschluss lesen: Erfolg = <<1>><<4>>, Fehler = <<24>>TEXT<<4>>
            await ReadUntilAsync(stream, EOT, ct);
            int first = stream.ReadByte();
            if (first == -1) return TransmissionResult.Fail("Server hat ohne Antwort geschlossen.");
            if (first == CAN) //CAN 
            {
                string err = await ReadUntilAsync(stream, EOT, ct);
                return TransmissionResult.Fail(err);
            }
            // Erfolg: restliches EOT schlucken (falls Server genau <<1>><<4>> sendet)
            //_ = stream.ReadByte();
           string text = await ReadUntilAsync(stream, EOT, ct);
            return TransmissionResult.Ok(text);
        }

        private static async Task WriteFrameAsync(NetworkStream s, string text, CancellationToken ct)
        {
            var utf8 = new UTF8Encoding(false);
            var bytes = utf8.GetBytes(text);
            await s.WriteAsync(new[] { SOH }, 0, 1, ct);
            await s.WriteAsync(bytes, 0, bytes.Length, ct);
            await s.WriteAsync(new[] { EOT }, 0, 1, ct);
            await s.FlushAsync(ct);
        }

        private static async Task<string> ReadFrameAsStringAsync(NetworkStream s, CancellationToken ct, bool expectSoh)
        {
            if (expectSoh)
            {
                int sb = s.ReadByte();
                if (sb != SOH) throw new IOException($"Startbyte fehlt/ist falsch: 0x{sb:X2}");
            }
            string text = await ReadUntilAsync(s, EOT, ct);
            return text;
        }

        private static async Task<string> ReadUntilAsync(NetworkStream s, byte terminator, CancellationToken ct)
        {
            using var ms = new MemoryStream();
            while (true)
            {
                int b = s.ReadByte();
                if (b == -1 || b == terminator) break;
                ms.WriteByte((byte)b);
                await Task.Yield();
            }
            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }
}
