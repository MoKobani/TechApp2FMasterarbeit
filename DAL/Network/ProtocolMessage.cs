using System.Diagnostics;
using System.Text;

namespace DAL.Network
{
    public sealed class ProtocolMessage
    {
        // Steuerzeichen
        public const byte SOH = 0x01;    // <<1>>
        public const byte RS = 0x1E;    // <<30>>
        public const byte EOT = 0x04;    // <<4>>
        public const byte CAN = 0x18;   // <<24>>

        private readonly string _payload; // UTF-8 Text mit RS als Trenner

        public ProtocolMessage(string payload) => _payload = payload ?? string.Empty;

        public byte[] ToFramedUtf8() // SOH + UTF8(payload) + EOT
        {
            var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            var body = utf8.GetBytes(_payload);
            var framed = new byte[body.Length + 2];
            framed[0] = SOH;
            Buffer.BlockCopy(body, 0, framed, 1, body.Length);
            framed[^1] = EOT;
            return framed;
        }

        public override string ToString() => _payload;

        // Hilfen zum Bauen des Payloads
        public static void AppendToken(StringBuilder sb, string? token)
        {
            sb.Append(token ?? string.Empty);
            sb.Append((char)RS); // echtes RS (0x1E), NICHT ein ▲-Symbol
        }
    }
}
