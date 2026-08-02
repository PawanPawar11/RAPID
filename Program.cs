using System;
using System.Threading;
using System.Threading.Tasks;
using RAPID.Commands;
using RAPID.Persistence;
using RAPID.PubSub;
using RAPID.Server;
using RAPID.Storage;

class Program
{
    static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();

        var db = new Database();
        var pubSub = new PubSubManager();
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
        expirationManager.Start(cts.Token);

        var server = new TcpServer(6379, db, pubSub, dispatcher);

        // Handle Ctrl+C and SIGINT/SIGTERM process signals for graceful shutdown
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            Console.WriteLine($"\n[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Server Shutdown] Received termination signal. Initiating graceful shutdown...");
            eventArgs.Cancel = true; // Prevent abrupt exit
            cts.Cancel();
        };

        try
        {
            await server.StartAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Expected on cancellation
        }
        finally
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Server Shutdown] Saving database snapshot before exit...");
            try
            {
                persistenceManager.Save(db);
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Server Shutdown] Persistence snapshot saved.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Server Shutdown] Failed to save snapshot: {ex.Message}");
            }

            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Server Shutdown] Server stopped cleanly. Goodbye!");
        }
    }
}