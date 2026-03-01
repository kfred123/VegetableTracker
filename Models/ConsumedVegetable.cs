using SQLite;

namespace VegetableTracker.Models;

public class ConsumedVegetable
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string VegetableId { get; set; } = string.Empty;

    public DateTime ConsumedAt { get; set; }
}
