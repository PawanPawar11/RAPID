using System;
using System.Threading.Tasks;
using RAPID.Commands;
using RAPID.Persistence;
using RAPID.Server;
using RAPID.Storage;

class Program
{
    static async Task Main(string[] args)
    {
        var db = new Database();
        var persistenceManager = new PersistenceManager("dump.json");

        // Load existing database snapshot from disk on startup
        int restoredKeys = persistenceManager.Load(db);
        if (restoredKeys > 0)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Persistence] Restored {restoredKeys} key(s) from dump.json.");
        }

        var dispatcher = new CommandDispatcher(persistenceManager);

        // Start background key expiration manager (runs every 1 second)
        var expirationManager = new ExpirationManager(db, TimeSpan.FromSeconds(1));
        expirationManager.Start();

        // Start TCP Server on port 6379
        var server = new TcpServer(6379, db, dispatcher);
        await server.StartAsync();
    }
}