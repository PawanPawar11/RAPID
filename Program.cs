using System;
using System.Threading.Tasks;
using RAPID.Commands;
using RAPID.Server;
using RAPID.Storage;

class Program
{
    static async Task Main(string[] args)
    {
        var db = new Database();
        var dispatcher = new CommandDispatcher();

        // Start background key expiration manager (runs every 1 second)
        var expirationManager = new ExpirationManager(db, TimeSpan.FromSeconds(1));
        expirationManager.Start();

        // Start TCP Server on port 6379
        var server = new TcpServer(6379, db, dispatcher);
        await server.StartAsync();
    }
}