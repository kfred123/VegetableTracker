using System.Windows.Input;
using VegetableTracker.Resources.Strings;
using VegetableTracker.Services;

namespace VegetableTracker;

public record SuggestionItem(string Id, string DisplayName);

public partial class SuggestionsPage : ContentPage
{
    private readonly DatabaseService _databaseService;

    public ICommand AddCommand { get; }

    public SuggestionsPage(DatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;
        AddCommand = new Command<SuggestionItem>(async item => await OnAddSuggestion(item));
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadSuggestionsAsync();
    }

    private async Task LoadSuggestionsAsync()
    {
        var since = DateTime.UtcNow.AddDays(-7);
        var consumed = await _databaseService.GetConsumedVegetablesAsync(since);
        var consumedIds = consumed.Select(v => v.VegetableId).Distinct(StringComparer.OrdinalIgnoreCase).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Get ALL consumed vegetables (all time) for sorting by frequency
        var allConsumed = await _databaseService.GetConsumedVegetablesAsync(DateTime.MinValue);

        // Build frequency and last-consumed maps
        var frequencyMap = allConsumed
            .GroupBy(v => v.VegetableId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        var lastConsumedMap = allConsumed
            .GroupBy(v => v.VegetableId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Max(v => v.ConsumedAt), StringComparer.OrdinalIgnoreCase);

        var resourceManager = AppResources.ResourceManager;

        // Collect all known vegetables (predefined + custom)
        var allVegetables = new List<SuggestionItem>();

        // Predefined
        var predefinedIds = new[]
        {
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
        };

        foreach (var id in predefinedIds)
        {
            allVegetables.Add(new SuggestionItem(id, resourceManager.GetString($"Vegetable_{id}") ?? id));
        }

        // Custom from DB
        var customList = await _databaseService.GetCustomVegetablesAsync();
        foreach (var c in customList)
        {
            allVegetables.Add(new SuggestionItem(c.Id, c.Name));
        }

        // Filter out already consumed in last 7 days
        var suggestions = allVegetables
            .Where(v => !consumedIds.Contains(v.Id))
            .OrderBy(v =>
            {
                // Sort by frequency (ascending — least consumed first)
                return frequencyMap.GetValueOrDefault(v.Id, 0);
            })
            .ThenByDescending(v =>
            {
                // Then by most recent consumption date (newest first)
                return lastConsumedMap.GetValueOrDefault(v.Id, DateTime.MinValue);
            })
            .ThenBy(v => v.DisplayName) // Then alphabetically
            .ToList();

        SuggestionsList.ItemsSource = suggestions;
    }

    private async Task OnAddSuggestion(SuggestionItem item)
    {
        await _databaseService.AddConsumedVegetableAsync(item.Id);
        await Shell.Current.GoToAsync("..");
    }
}
