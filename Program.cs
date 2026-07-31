using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

TcpListener listener = new TcpListener(IPAddress.Any, 6379);

// Shared in-memory key-value store
ConcurrentDictionary<string, string> store = new();

listener.Start();

Console.WriteLine("Redis server started on port 6379...");

while (true)
{
    TcpClient client = listener.AcceptTcpClient();

    Console.WriteLine("New client connected.");

    _ = Task.Run(() => HandleClient(client, store));
}

static void HandleClient(TcpClient client, ConcurrentDictionary<string, string> store)
{
    using (client)
    using (NetworkStream stream = client.GetStream())
    {
        byte[] buffer = new byte[1024];

        while (true)
        {
            int bytesRead;

            try
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
            }
            catch
            {
                break;
            }

            // Client disconnected
            if (bytesRead == 0)
            {
                break;
            }

            string message = Encoding.UTF8
                .GetString(buffer, 0, bytesRead)
                .Trim();

            Console.WriteLine($"Received: {message}");

            string[] parts = message.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries
            );

            if (parts.Length == 0)
            {
                continue;
            }

            string command = parts[0].ToUpperInvariant();

            switch (command)
            {
                case "SET":
                    {
                        if (parts.Length < 3)
                        {
                            WriteError(stream, "Usage: SET <key> <value>");
                            break;
                        }

                        string key = parts[1];

                        // Supports values containing spaces
                        string value = string.Join(" ", parts.Skip(2));

                        store[key] = value;

                        WriteOk(stream);
                        break;
                    }

                case "GET":
                    {
                        if (parts.Length < 2)
                        {
                            WriteError(stream, "Usage: GET <key>");
                            break;
                        }

                        string key = parts[1];

                        if (store.TryGetValue(key, out string? value))
                        {
                            WriteBulkString(stream, value);
                        }
                        else
                        {
                            WriteNullBulkString(stream);
                        }

                        break;
                    }

                default:
                    {
                        WriteError(stream, $"Unknown command '{command}'");
                        break;
                    }
            }
        }
    }

    Console.WriteLine("Client disconnected.");
}

static void WriteOk(NetworkStream stream)
{
    byte[] response = Encoding.UTF8.GetBytes("+OK\r\n");
    stream.Write(response, 0, response.Length);
}

static void WriteBulkString(NetworkStream stream, string value)
{
    string response = $"${value.Length}\r\n{value}\r\n";

    byte[] bytes = Encoding.UTF8.GetBytes(response);

    stream.Write(bytes, 0, bytes.Length);
}

static void WriteNullBulkString(NetworkStream stream)
{
    byte[] response = Encoding.UTF8.GetBytes("$-1\r\n");
    stream.Write(response, 0, response.Length);
}

static void WriteError(NetworkStream stream, string message)
{
    byte[] response = Encoding.UTF8.GetBytes($"-ERR {message}\r\n");
    stream.Write(response, 0, response.Length);
}