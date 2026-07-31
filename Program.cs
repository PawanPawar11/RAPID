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

    Task task = Task.Run(() =>
    {
        using NetworkStream stream = client.GetStream();

        byte[] buffer = new byte[1024];

        while (true)
        {
            int bytesRead = stream.Read(buffer, 0, buffer.Length);

            // Client disconnected
            if (bytesRead == 0)
            {
                break;
            }

            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

            Console.WriteLine($"Received: {message}");

            string[] parts = message.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries
            );

            if (parts.Length == 0)
            {
                continue;
            }

            string command = parts[0].ToUpper();

            switch (command)
            {
                case "SET":
                    {
                        if (parts.Length < 3)
                        {
                            WriteResponse(stream, "ERROR: Usage: SET <key> <value>");
                            break;
                        }

                        string key = parts[1];

                        // Allows values containing spaces
                        string value = string.Join(" ", parts.Skip(2));

                        store[key] = value;

                        WriteResponse(stream, "OK");
                        break;
                    }

                case "GET":
                    {
                        if (parts.Length < 2)
                        {
                            WriteResponse(stream, "ERROR: Usage: GET <key>");
                            break;
                        }

                        string key = parts[1];

                        if (store.TryGetValue(key, out string? value))
                        {
                            WriteResponse(stream, value);
                        }
                        else
                        {
                            WriteResponse(stream, "(nil)");
                        }

                        break;
                    }

                default:
                    {
                        WriteResponse(stream, "ERROR: Unknown command");
                        break;
                    }
            }
        }

        client.Close();

        Console.WriteLine("Client disconnected.");
    });
}

static void WriteResponse(NetworkStream stream, string response)
{
    byte[] responseBytes = Encoding.UTF8.GetBytes(response + "\r\n");
    stream.Write(responseBytes, 0, responseBytes.Length);
}