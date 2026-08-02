using System;
using System.Net.Sockets;
using System.Text;
using RAPID.Commands;
using RAPID.Storage;

namespace RAPID.Server;

public class ClientHandler
{
    private readonly Database _db;
    private readonly CommandDispatcher _dispatcher;

    public ClientHandler(Database db, CommandDispatcher dispatcher)
    {
        _db = db;
        _dispatcher = dispatcher;
    }

    public void HandleClient(TcpClient client)
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

                string response = _dispatcher.Dispatch(_db, input);

                if (!string.IsNullOrEmpty(response))
                {
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseBytes, 0, responseBytes.Length);
                }
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
