using System;
using System.Threading;
using System.Threading.Tasks;

namespace RAPID.Storage;

public class ExpirationManager
{
    private readonly Database _db;
    private readonly TimeSpan _interval;

    public ExpirationManager(Database db, TimeSpan interval)
    {
        _db = db;
        _interval = interval;
    }

    public void Start(CancellationToken cancellationToken = default)
    {
        Task.Run(async () =>
        {
            using var timer = new PeriodicTimer(_interval);
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                int cleaned = _db.CleanupExpiredKeys();
                if (cleaned > 0)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Active Expiration] Cleaned {cleaned} expired key(s).");
                }
            }
        }, cancellationToken);
    }
}
