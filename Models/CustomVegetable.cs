using SQLite;

namespace VegetableTracker.Models;

public class CustomVegetable
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
