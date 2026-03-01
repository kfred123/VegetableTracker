using SQLite;
using VegetableTracker.Models;

namespace VegetableTracker.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _database;

    private async Task<SQLiteAsyncConnection> GetDatabaseAsync()
    {
        if (_database is not null)
            return _database;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "vegetables.db3");
        _database = new SQLiteAsyncConnection(dbPath);
        await _database.CreateTableAsync<ConsumedVegetable>();
        await _database.CreateTableAsync<CustomVegetable>();
        return _database;
    }

    public async Task AddConsumedVegetableAsync(string vegetableId)
    {
        var db = await GetDatabaseAsync();
        var entry = new ConsumedVegetable
        {
            VegetableId = vegetableId,
            ConsumedAt = DateTime.UtcNow
        };
        await db.InsertAsync(entry);
    }

    public async Task<List<ConsumedVegetable>> GetConsumedVegetablesAsync(DateTime since)
    {
        var db = await GetDatabaseAsync();
        return await db.Table<ConsumedVegetable>()
            .Where(v => v.ConsumedAt >= since)
            .ToListAsync();
    }

    public async Task<CustomVegetable> AddCustomVegetableAsync(string name)
    {
        var db = await GetDatabaseAsync();
        var entry = new CustomVegetable
        {
            Id = $"custom_{Guid.NewGuid():N}",
            Name = name.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        await db.InsertAsync(entry);
        return entry;
    }

    public async Task<List<CustomVegetable>> GetCustomVegetablesAsync()
    {
        var db = await GetDatabaseAsync();
        return await db.Table<CustomVegetable>().ToListAsync();
    }

    public async Task<string?> GetCustomVegetableNameAsync(string id)
    {
        var db = await GetDatabaseAsync();
        var entry = await db.Table<CustomVegetable>()
            .Where(v => v.Id == id)
            .FirstOrDefaultAsync();
        return entry?.Name;
    }
}
