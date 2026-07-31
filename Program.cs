using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    // Thread-safe in-memory key-value store
    private static readonly ConcurrentDictionary<string, string> _store =
        new ConcurrentDictionary<string, string>();

    static async Task Main(string[] args)
    {
        // Listen on port 6379
        int port = 6379;
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        // Logging
        Log($"Redis Server started and listening on port {port}...");

        try
        {
            while (true)
            {
                // Accept incoming client
                TcpClient client = await listener.AcceptTcpClientAsync();

                // Support multiple clients using Task.Run
                _ = Task.Run(() => HandleClient(client));
            }
        }
        finally
        {
            listener.Stop();
        }
    }

    private static void HandleClient(TcpClient client)
    {
        string clientEndPoint = client.Client.RemoteEndPoint?.ToString() ?? "Unknown";

        // Logging connection
        Log($"Client connected: {clientEndPoint}");

        using NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        try
        {
            while (true)
            {
                // Read text from socket
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                // Handle client disconnects (bytesRead == 0 means client closed connection)
                if (bytesRead == 0)
                {
                    Log($"Client disconnected gracefully: {clientEndPoint}");
                    break;
                }

                string input = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                // Print received command
                Log($"[{clientEndPoint}] Received command: {input}");

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                // Simple parser split by spaces
                string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string command = parts[0].ToUpper();

                string response;

                switch (command)
                {
                    // Parse SET key value
                    case "SET":
                        if (parts.Length >= 3)
                        {
                            string key = parts[1];
                            string value = string.Join(" ", parts, 2, parts.Length - 2);

                            _store[key] = value;

                            // Return OK
                            response = "+OK\r\n";
                        }
                        else
                        {
                            response = "-ERR wrong number of arguments for 'set' command\r\n";
                        }

                        break;

                    // Parse GET key
                    case "GET":
                        if (parts.Length >= 2)
                        {
                            string key = parts[1];

                            if (_store.TryGetValue(key, out string? val))
                            {
                                // Return response (RESP bulk string format)
                                response = $"${Encoding.UTF8.GetByteCount(val)}\r\n{val}\r\n";
                            }
                            else
                            {
                                // Key not found (null bulk string)
                                response = "$-1\r\n";
                            }
                        }
                        else
                        {
                            response = "-ERR wrong number of arguments for 'get' command\r\n";
                        }

                        break;

                    case "PING":
                        response = "+PONG\r\n";
                        break;

                    default:
                        response = $"-ERR unknown command '{command}'\r\n";
                        break;
                }

                // Send response back
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                stream.Write(responseBytes, 0, responseBytes.Length);
            }
        }
        catch (Exception ex)
        {
            // Handle unexpected client disconnects or socket errors
            Log($"Client error/disconnect [{clientEndPoint}]: {ex.Message}");
        }
        finally
        {
            client.Close();
        }
    }

    private static void Log(string message)
    {
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
    }
}