using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    private static readonly ConcurrentDictionary<string, string> _store =
        new ConcurrentDictionary<string, string>();

    static async Task Main(string[] args)
    {
        int port = 6379;
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        Log($"Redis Server started and listening on port {port}...");

        try
        {
            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
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
        Log($"Client connected: {clientEndPoint}");

        using NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        try
        {
            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                {
                    Log($"Client disconnected gracefully: {clientEndPoint}");
                    break;
                }

                string input = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                Log($"[{clientEndPoint}] Received command: {input}");

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string command = parts[0].ToUpper();

                string response;

                switch (command)
                {
                    case "SET":
                        if (parts.Length >= 3)
                        {
                            string key = parts[1];
                            string value = string.Join(" ", parts, 2, parts.Length - 2);
                            _store[key] = value;
                            response = "+OK\r\n";
                        }
                        else
                        {
                            response = "-ERR wrong number of arguments for 'set' command\r\n";
                        }
                        break;

                    case "GET":
                        if (parts.Length >= 2)
                        {
                            string key = parts[1];
                            if (_store.TryGetValue(key, out string? val))
                            {
                                response = $"${Encoding.UTF8.GetByteCount(val)}\r\n{val}\r\n";
                            }
                            else
                            {
                                response = "$-1\r\n";
                            }
                        }
                        else
                        {
                            response = "-ERR wrong number of arguments for 'get' command\r\n";
                        }
                        break;

                    case "DEL":
                        if (parts.Length >= 2)
                        {
                            int deletedCount = 0;

                            for (int i = 1; i < parts.Length; i++)
                            {
                                if (_store.TryRemove(parts[i], out _))
                                {
                                    deletedCount++;
                                }
                            }

                            response = $":{deletedCount}\r\n";
                        }
                        else
                        {
                            response = "-ERR wrong number of arguments for 'del' command\r\n";
                        }
                        break;

                    case "EXISTS":
                        if (parts.Length >= 2)
                        {
                            int existingCount = 0;

                            for (int i = 1; i < parts.Length; i++)
                            {
                                if (_store.ContainsKey(parts[i]))
                                {
                                    existingCount++;
                                }
                            }

                            response = $":{existingCount}\r\n";
                        }
                        else
                        {
                            response = "-ERR wrong number of arguments for 'exists' command\r\n";
                        }
                        break;

                    case "PING":
                        response = "+PONG\r\n";
                        break;

                    default:
                        response = $"-ERR unknown command '{command}'\r\n";
                        break;
                }

                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                stream.Write(responseBytes, 0, responseBytes.Length);
            }
        }
        catch (Exception ex)
        {
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