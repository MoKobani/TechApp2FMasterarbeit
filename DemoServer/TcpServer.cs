using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

const int port = 6020;
const string SupportedVersion = "1.0";
const string ExpectedDescriptor = "2:1";
const byte StatusOk = 0x01;
const byte StatusError = 0x24;

var listener = new TcpListener(IPAddress.Loopback, port);
listener.Start();

Console.WriteLine($"[Server] Lausche auf Port {port}. Tippe 'exit' zum Beenden.");

using var cts = new CancellationTokenSource();

_ = Task.Run(() =>
{
    while (true)
    {
        var cmd = Console.ReadLine();
        if (string.Equals(cmd, "exit", StringComparison.OrdinalIgnoreCase))
        {
            cts.Cancel();
            break;
        }
    }
});

try
{
    while (!cts.IsCancellationRequested)
    {
        TcpClient client;
        try
        {
            client = await listener.AcceptTcpClientAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            break;
        }

        await HandleClientAsync(client, cts.Token);
    }
}
finally
{
    listener.Stop();
    Console.WriteLine("[Server] Beendet.");
    Console.ReadKey();
}

static async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
{
    Console.WriteLine("[Server] Client verbunden.");

    using (client)
    using (NetworkStream netStream = client.GetStream())
    {
        StreamReader? reader = null;
        StreamWriter? writer = null;
        var descriptorConfirmed = false;

        try
        {
            await SendFramedStringAsync(netStream, SupportedVersion, cancellationToken);
            Console.WriteLine($"[Server] Protokollversion {SupportedVersion} gesendet.");

            var descriptor = await ReadFramedStringAsync(netStream, cancellationToken);
            if (descriptor is null)
            {
                await SendDescriptorResponseAsync(netStream, success: false, "Kein Descriptor empfangen.", cancellationToken);
                return;
            }

            if (!string.Equals(descriptor, ExpectedDescriptor, StringComparison.Ordinal))
            {
                Console.WriteLine($"[Server] Unbekannter Descriptor: {descriptor}");
                await SendDescriptorResponseAsync(netStream, success: false, $"Descriptor '{descriptor}' wird nicht unterstützt.", cancellationToken);
                return;
            }

            await SendDescriptorResponseAsync(netStream, success: true, null, cancellationToken);
            descriptorConfirmed = true;
            Console.WriteLine($"[Server] Descriptor bestätigt: {descriptor}");

            reader = new StreamReader(netStream, Encoding.UTF8, leaveOpen: true);
            writer = new StreamWriter(netStream, Encoding.UTF8, bufferSize: 1024, leaveOpen: true)
            {
                AutoFlush = true
            };

            var message = await ReadFramedMessageAsync(reader, cancellationToken);
            if (message is null)
            {
                await SendNotOkAsync(writer, "Datennachricht war leer oder unvollständig.");
                return;
            }

            LogSection("Header", message.Value.Header);
            LogSection("Daten", message.Value.Body);

            await writer.WriteLineAsync("OK");
            Console.WriteLine("[Server] Verarbeitung abgeschlossen, OK gesendet.");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("[Server] Verbindung wegen Server-Stopp beendet.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Server] Fehler: {ex.Message}");
            try
            {
                if (descriptorConfirmed && writer is not null)
                {
                    await SendNotOkAsync(writer, $"Serverfehler: {ex.Message}");
                }
                else
                {
                    await SendDescriptorResponseAsync(netStream, success: false, $"Serverfehler: {ex.Message}", cancellationToken);
                }
            }
            catch
            {
                // Verbindung bereits getrennt
            }
        }
        finally
        {
            reader?.Dispose();
            writer?.Dispose();
        }
    }
}

static async Task SendNotOkAsync(StreamWriter writer, string message)
{
    await writer.WriteLineAsync("NOT OK");
    await writer.WriteLineAsync(message);
    Console.WriteLine($"[Server] NOT OK: {message}");
}

static async Task<ReceivedMessage?> ReadFramedMessageAsync(StreamReader reader, CancellationToken cancellationToken)
{
    var start = await ReadCharAsync(reader, cancellationToken);
    if (start is null)
    {
        return null;
    }

    if (start != ProtocolChars.StartOfHeading)
    {
        throw new InvalidOperationException("Unerwarteter Nachrichtenbeginn – SOH (0x01) erwartet.");
    }

    var headerBuilder = new StringBuilder();
    while (true)
    {
        var ch = await ReadCharAsync(reader, cancellationToken);
        if (ch is null)
        {
            return null;
        }

        if (ch == ProtocolChars.StartOfText)
        {
            break;
        }

        headerBuilder.Append(ch.Value);
    }

    var bodyBuilder = new StringBuilder();
    while (true)
    {
        var ch = await ReadCharAsync(reader, cancellationToken);
        if (ch is null)
        {
            return null;
        }

        if (ch == ProtocolChars.EndOfTransmission)
        {
            break;
        }

        bodyBuilder.Append(ch.Value);
    }

    return new ReceivedMessage(headerBuilder.ToString(), bodyBuilder.ToString());
}

static async Task<char?> ReadCharAsync(StreamReader reader, CancellationToken cancellationToken)
{
    var buffer = new char[1];
    int read = await reader.ReadAsync(buffer.AsMemory(0, 1), cancellationToken);
    if (read == 0)
    {
        return null;
    }

    return buffer[0];
}

static void LogSection(string sectionName, string section)
{
    Console.WriteLine($"[Server] {sectionName}:");
    if (string.IsNullOrEmpty(section))
    {
        Console.WriteLine("  (leer)");
        return;
    }

    var records = section.Split(ProtocolChars.RecordSeparator);
    foreach (var record in records)
    {
        if (string.IsNullOrEmpty(record))
        {
            continue;
        }

        var separatorIndex = record.IndexOf(ProtocolChars.UnitSeparator);
        if (separatorIndex >= 0)
        {
            var key = record[..separatorIndex];
            var value = record[(separatorIndex + 1)..];
            Console.WriteLine($"  {key} = {value}");
        }
        else
        {
            Console.WriteLine($"  {record}");
        }
    }
}

readonly record struct ReceivedMessage(string Header, string Body);

static async Task SendFramedStringAsync(NetworkStream stream, string value, CancellationToken cancellationToken)
{
    var contentBytes = Encoding.UTF8.GetBytes(value);
    var payload = new byte[contentBytes.Length + 2];
    payload[0] = (byte)ProtocolChars.StartOfHeading;
    Buffer.BlockCopy(contentBytes, 0, payload, 1, contentBytes.Length);
    payload[^1] = (byte)ProtocolChars.EndOfTransmission;

    await stream.WriteAsync(payload.AsMemory(0, payload.Length), cancellationToken);
}

static async Task<string?> ReadFramedStringAsync(NetworkStream stream, CancellationToken cancellationToken)
{
    var buffer = new byte[1];
    var read = await stream.ReadAsync(buffer.AsMemory(0, 1), cancellationToken);
    if (read == 0)
    {
        return null;
    }

    if (buffer[0] != (byte)ProtocolChars.StartOfHeading)
    {
        throw new InvalidDataException($"Unerwarteter Beginn der Nachricht: 0x{buffer[0]:X2}.");
    }

    using var content = new MemoryStream();

    while (true)
    {
        read = await stream.ReadAsync(buffer.AsMemory(0, 1), cancellationToken);
        if (read == 0)
        {
            return null;
        }

        if (buffer[0] == (byte)ProtocolChars.EndOfTransmission)
        {
            break;
        }

        content.WriteByte(buffer[0]);
    }

    return Encoding.UTF8.GetString(content.ToArray());
}

static async Task SendDescriptorResponseAsync(NetworkStream stream, bool success, string? message, CancellationToken cancellationToken)
{
    if (success)
    {
        await stream.WriteAsync(new[] { StatusOk }, cancellationToken);
        return;
    }

    await stream.WriteAsync(new[] { StatusError }, cancellationToken);

    if (!string.IsNullOrEmpty(message))
    {
        await SendFramedStringAsync(stream, message, cancellationToken);
    }
}

static class ProtocolChars
{
    public const char StartOfHeading = (char)1;
    public const char StartOfText = (char)2;
    public const char EndOfTransmission = (char)4;
    public const char RecordSeparator = (char)30;
    public const char UnitSeparator = (char)31;
}
