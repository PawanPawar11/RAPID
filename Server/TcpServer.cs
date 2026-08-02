using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using RAPID.Commands;
using RAPID.PubSub;
using RAPID.Storage;

namespace RAPID.Server;

public class TcpServer
{
    private readonly int _port;
    private readonly Database _db;
    private readonly PubSubManager _pubSub;
    private readonly CommandDispatcher _dispatcher;
    private readonly ClientHandler _clientHandler;
    private readonly ConcurrentDictionary<TcpClient, byte> _activeClients = new();

    public TcpServer(int port, Database db, PubSubManager pubSub, CommandDispatcher dispatcher)
    {
        _port = port;
        _db = db;
        _pubSub = pubSub;
        _dispatcher = dispatcher;
        _clientHandler = new ClientHandler(_db, _pubSub, _dispatcher);
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        TcpListener listener = new TcpListener(IPAddress.Any, _port);
        listener.Start();

        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Redis Server started and listening on port {_port}...");

        using var reg = cancellationToken.Register(() =>
        {
            try
            {
                listener.Stop();
            }
            catch { }
        });

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client;
                try
                {
                    client = await listener.AcceptTcpClientAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }

                _activeClients.TryAdd(client, 0);

                _ = Task.Run(() =>
                {
                    try
                    {
                        _clientHandler.HandleClient(client);
                    }
                    finally
                    {
                        _activeClients.TryRemove(client, out _);
                    }
                }, cancellationToken);
            }
        }
        finally
        {
            listener.Stop();
            CloseAllClients();
        }
    }

    public void CloseAllClients()
    {
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Closing {_activeClients.Count} active client connection(s)...");
        foreach (var client in _activeClients.Keys)
        {
            try
            {
                client.Close();
                client.Dispose();
            }
            catch { }
        }
        _activeClients.Clear();
    }
}
