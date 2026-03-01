using System.Resources;
using VegetableTracker.Resources.Strings;
using VegetableTracker.Services;

namespace VegetableTracker;

public record VegetableItem(string Id, string DisplayName);

public partial class SelectVegetablePage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private List<VegetableItem> _allVegetables = [];

    private static readonly string[] VegetableIds =
    [
        "artichoke", "arugula", "asparagus", "beet", "bell_pepper",
        "bok_choy", "broccoli", "brussels_sprouts", "butternut_squash", "cabbage",
        "carrot", "cauliflower", "celery", "chard", "collard_greens",
        "corn", "cucumber", "eggplant", "endive", "fennel",
        "garlic", "green_beans", "kale", "kohlrabi", "leek",
        "lettuce", "mushroom", "okra", "onion", "parsnip",
        "peas", "potato", "pumpkin", "radish", "rutabaga",
        "shallot", "snap_peas", "spinach", "sweet_potato", "tomato",
        "turnip", "watercress", "zucchini", "radicchio", "daikon",
        "bamboo_shoots", "bean_sprouts", "celeriac", "chayote", "jicama"
    ];

    public SelectVegetablePage(DatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadVegetablesAsync();
    }

    private async Task LoadVegetablesAsync()
    {
        var resourceManager = AppResources.ResourceManager;

        // Predefined vegetables
        var predefined = VegetableIds
            .Select(id => new VegetableItem(id, resourceManager.GetString($"Vegetable_{id}") ?? id));

        // Custom vegetables from DB
        var customList = await _databaseService.GetCustomVegetablesAsync();
        var custom = customList.Select(c => new VegetableItem(c.Id, c.Name));

        // Merge and sort
        _allVegetables = predefined.Concat(custom)
            .OrderBy(v => v.DisplayName)
            .ToList();

        VegetableList.ItemsSource = _allVegetables;
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        var filter = e.NewTextValue?.Trim() ?? string.Empty;
        VegetableList.ItemsSource = string.IsNullOrEmpty(filter)
            ? _allVegetables
            : _allVegetables.Where(v => v.DisplayName.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private async void OnVegetableSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not VegetableItem selected)
            return;

        // Reset selection so the same item can be tapped again later
        VegetableList.SelectedItem = null;

        await _databaseService.AddConsumedVegetableAsync(selected.Id);
        await Shell.Current.GoToAsync("..");
    }

    private async void OnAddCustomClicked(object? sender, EventArgs e)
    {
        var name = await DisplayPromptAsync(
            AppResources.AddCustomVegetable,
            AppResources.EnterVegetableName,
            AppResources.Add,
            AppResources.Cancel);

        if (string.IsNullOrWhiteSpace(name))
            return;

        // Check for duplicate name (case-insensitive)
        var exists = _allVegetables.Any(v =>
            v.DisplayName.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));

        if (exists)
        {
            await DisplayAlertAsync(
                AppResources.AddCustomVegetable,
                AppResources.VegetableAlreadyExists,
                "OK");
            return;
        }

        await _databaseService.AddCustomVegetableAsync(name);
        await LoadVegetablesAsync();
    }
}
