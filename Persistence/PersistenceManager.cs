using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RAPID.Storage;

namespace RAPID.Persistence;

public class PersistenceManager
{
    private readonly string _filePath;
    private readonly string _tempFilePath;
    private int _isBgsaveRunning = 0;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public PersistenceManager(string filePath = "dump.json")
    {
        _filePath = filePath;
        _tempFilePath = $"{filePath}.tmp";
    }

    public bool IsBgsaveRunning => Volatile.Read(ref _isBgsaveRunning) == 1;

    public void Save(Database db)
    {
        var keys = db.CreateSnapshot();
        var snapshotDto = new DatabaseSnapshotDto
        {
            SavedAtUtc = DateTime.UtcNow,
            Keys = keys
        };

        string json = JsonSerializer.Serialize(snapshotDto, _jsonOptions);

        // Write to temporary file first to prevent corrupt partially written files
        File.WriteAllText(_tempFilePath, json);

        // Atomically replace target dump file
        File.Move(_tempFilePath, _filePath, overwrite: true);
    }

    public bool SaveBackground(Database db)
    {
        if (Interlocked.CompareExchange(ref _isBgsaveRunning, 1, 0) != 0)
        {
            return false; // Background save already running
        }

        Task.Run(() =>
        {
            try
            {
                Save(db);
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [BGSAVE] Background saving terminated with success.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [BGSAVE] Background save failed: {ex.Message}");
            }
            finally
            {
                Interlocked.Exchange(ref _isBgsaveRunning, 0);
            }
        });

        return true;
    }

    public int Load(Database db)
    {
        if (!File.Exists(_filePath))
        {
            return 0;
        }

        try
        {
            string json = File.ReadAllText(_filePath);
            var snapshotDto = JsonSerializer.Deserialize<DatabaseSnapshotDto>(json);

            if (snapshotDto?.Keys != null)
            {
                return db.LoadSnapshot(snapshotDto.Keys);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Persistence] Error loading snapshot from disk: {ex.Message}");
        }

        return 0;
    }
}
