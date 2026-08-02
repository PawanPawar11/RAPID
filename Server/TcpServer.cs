using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using RAPID.Commands;
using RAPID.Storage;

namespace RAPID.Server;

public class TcpServer
{
    private readonly int _port;
    private readonly Database _db;
    private readonly CommandDispatcher _dispatcher;
    private readonly ClientHandler _clientHandler;

    public TcpServer(int port, Database db, CommandDispatcher dispatcher)
    {
        _port = port;
        _db = db;
        _dispatcher = dispatcher;
        _clientHandler = new ClientHandler(_db, _dispatcher);
    }

    public async Task StartAsync()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, _port);
        listener.Start();

        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Redis Server started and listening on port {_port}...");

        try
        {
            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                _ = Task.Run(() => _clientHandler.HandleClient(client));
            }
        }
        finally
        {
            listener.Stop();
        }
    }
}
