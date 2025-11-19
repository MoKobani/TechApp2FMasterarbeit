using BLL.Models;
using DAL.Network;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class NetworkService
    {
        private const string DefaultProtocolVersion = "0.0.1";
        private const string BomEntriesKey = "BomEntries";

        private static readonly (string SourceKey, string TargetKey)[] BaseFieldOrder =
        {
        ("Artikelnummer", "swd"),
        ("Länge",         "yllaenge"),
        ("Breite",        "ylbreite"),
        ("Höhe",          "ylhoehe"),
        ("Gewicht",       "weight"),
        ("Lagereinheit",  "SU"),
        ("Beschreibung",  "ybem"),
        ("Lagerplatz",    "receiptLoc"),
        ("Lieferant",     "vendor"),
        ("Beschaffungsart","procureMode"),
        ("Abteilung",     "dept"),
        ("JPG-Pfad",    "yfauf"),
        ("Dateipfad",     "ycad3d"),
        ("Oberfläche",     "yzfarbe")
        // ggf. ("Dateipfad","Dateipfad") ergänzen, falls benötigt
        // Hier können die Feldnamen angepasst werden, falls nötig
    };

        public ProtocolMessage BuildPayload(Dictionary<string, object> data, string user)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));

            var sb = new StringBuilder();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // HEAD (Key/Value)
            foreach (var (src, tgt) in BaseFieldOrder)
            {
                seen.Add(src);
                ProtocolMessage.AppendToken(sb, tgt);
                ProtocolMessage.AppendToken(sb, TryFormatValue(data.TryGetValue(src, out var v) ? v : null));
            }


            // weitere Felder (außer BomEntries/Bauteilart)
            foreach (var kv in data)
            {
                if (seen.Contains(kv.Key) || string.Equals(kv.Key, BomEntriesKey, StringComparison.Ordinal) ||
                    string.Equals(kv.Key, "Bauteilart", StringComparison.OrdinalIgnoreCase))
                    continue;
                ProtocolMessage.AppendToken(sb, kv.Key);
                ProtocolMessage.AppendToken(sb, TryFormatValue(kv.Value));
                seen.Add(kv.Key);
            }


            DateTime now = DateTime.Now;
            string thisDay = now.ToString("dd.MM.yy");
            ProtocolMessage.AppendToken(sb, "comments");
            ProtocolMessage.AppendToken(sb, $"{user} {thisDay}");

            // TAB-Bereich (BOM)
            if (data.TryGetValue(BomEntriesKey, out var bomObj) && bomObj is IEnumerable<BomEntry> bom)
            {
                ProtocolMessage.AppendToken(sb, "TAB"); // <== WICHTIG
                foreach (var e in bom)
                {
                    // Hier können die Feldnamen angepasst werden, falls nötig
                    ProtocolMessage.AppendToken(sb, "prodListElem"); ProtocolMessage.AppendToken(sb, e.ItemNumber);

                    if (!string.IsNullOrWhiteSpace(e.Length))
                    {
                        ProtocolMessage.AppendToken(sb, "length"); ProtocolMessage.AppendToken(sb, e.Length);
                    }
                    if (!string.IsNullOrWhiteSpace(e.Width))
                    {
                        ProtocolMessage.AppendToken(sb, "width"); ProtocolMessage.AppendToken(sb, TryFormatValue(e.Width));
                    }

                    if (!string.IsNullOrWhiteSpace(e.Amount))
                    {
                        ProtocolMessage.AppendToken(sb, "elemqty"); ProtocolMessage.AppendToken(sb, TryFormatValue(e.Amount));
                    }



                    //ProtocolMessage.AppendToken(sb, "length"); ProtocolMessage.AppendToken(sb, TryFormatValue(e.Length)); 
                    //ProtocolMessage.AppendToken(sb, "elemLMU"); ProtocolMessage.AppendToken(sb, e.LengthUnit);
                    //ProtocolMessage.AppendToken(sb, "width"); ProtocolMessage.AppendToken(sb, TryFormatValue(e.Width));
                    //ProtocolMessage.AppendToken(sb, "elemWMU"); ProtocolMessage.AppendToken(sb, e.WidthUnit);
                    //ProtocolMessage.AppendToken(sb, "elemqty"); ProtocolMessage.AppendToken(sb, TryFormatValue(e.Amount));
                    //ProtocolMessage.AppendToken(sb, "countUnit"); ProtocolMessage.AppendToken(sb, e.AmountUnit);

                }
            }

            return new ProtocolMessage(sb.ToString());
        }

        public async Task<TransmissionResult> SendDataAsync(ProtocolMessage message, string serverIp, int port, string dbDescriptor, CancellationToken ct = default)
        {
            var sender = new TcpSender();
            return await sender.SendDataViaTcpAsync(message, dbDescriptor, serverIp, port, DefaultProtocolVersion, ct);
        }

        private static string TryFormatValue(object? v) =>
            v switch
            {
                null => string.Empty,
                IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
                _ => v.ToString() ?? string.Empty
            };
    }
}
