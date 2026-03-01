using VegetableTracker.Resources.Strings;
using VegetableTracker.Services;

namespace VegetableTracker;

public record ConsumedVegetableItem(string DisplayName, string DateText);

public partial class DetailsPage : ContentPage
{
    private readonly DatabaseService _databaseService;

    public DetailsPage(DatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDetailsAsync();
    }

    private async Task LoadDetailsAsync()
    {
        var since = DateTime.UtcNow.AddDays(-7);
        var consumed = await _databaseService.GetConsumedVegetablesAsync(since);
        var resourceManager = AppResources.ResourceManager;

        var items = new List<ConsumedVegetableItem>();
        foreach (var v in consumed.OrderByDescending(v => v.ConsumedAt))
        {
            string displayName;
            if (v.VegetableId.StartsWith("custom_", StringComparison.Ordinal))
            {
                displayName = await _databaseService.GetCustomVegetableNameAsync(v.VegetableId) ?? v.VegetableId;
            }
            else
            {
                displayName = resourceManager.GetString($"Vegetable_{v.VegetableId}") ?? v.VegetableId;
            }

            items.Add(new ConsumedVegetableItem(displayName, v.ConsumedAt.ToLocalTime().ToString("ddd, dd MMM")));
        }

        ConsumedList.ItemsSource = items;
    }
}
