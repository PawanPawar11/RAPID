using System;
using System.Net.Sockets;
using System.Text;
using RAPID.Commands;
using RAPID.PubSub;
using RAPID.Storage;

namespace RAPID.Server;

public class ClientHandler
{
    private readonly Database _db;
    private readonly PubSubManager _pubSub;
    private readonly CommandDispatcher _dispatcher;

    public ClientHandler(Database db, PubSubManager pubSub, CommandDispatcher dispatcher)
    {
        _db = db;
        _pubSub = pubSub;
        _dispatcher = dispatcher;
    }

    public void HandleClient(TcpClient client)
    {
        string clientEndPoint = client.Client.RemoteEndPoint?.ToString() ?? "Unknown";
        Log($"Client connected: {clientEndPoint}");

        using NetworkStream stream = client.GetStream();
        var session = new ClientSession(clientEndPoint, stream);
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

                var context = new CommandContext(_db, _pubSub, session);
                string response = _dispatcher.Dispatch(context, input);

                if (!string.IsNullOrEmpty(response))
                {
                    session.SendRawResponse(response);
                }
            }
        }
        catch (Exception ex)
        {
            Log($"Client error/disconnect [{clientEndPoint}]: {ex.Message}");
        }
        finally
        {
            // Automatic Pub/Sub cleanup when client disconnects
            _pubSub.UnsubscribeAll(session);
            client.Close();
        }
    }

    private static void Log(string message)
    {
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
    }
}
