using VegetableTracker.Resources.Strings;
using VegetableTracker.Services;

namespace VegetableTracker;

public partial class MainPage : ContentPage
{
    private const int WeeklyGoal = 30;
    private readonly DatabaseService _databaseService;

    public MainPage(DatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadProgressAsync();
    }

    private async Task LoadProgressAsync()
    {
        var since = DateTime.UtcNow.AddDays(-7);
        var consumed = await _databaseService.GetConsumedVegetablesAsync(since);
        var distinctCount = consumed.Select(v => v.VegetableId).Distinct(StringComparer.OrdinalIgnoreCase).Count();

        if (distinctCount == 0)
        {
            IntroLabel.IsVisible = true;
            ProgressSection.IsVisible = false;
        }
        else
        {
            IntroLabel.IsVisible = false;
            ProgressSection.IsVisible = true;

            var progress = Math.Min((double)distinctCount / WeeklyGoal, 1.0);
            TachoGauge.Progress = progress;
            TachoGauge.CurrentValue = distinctCount;
            TachoGauge.MaxValue = WeeklyGoal;
        }
    }

    private async void OnSeeDetailsTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("DetailsPage");
    }

    private async void OnShowSuggestionsTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("SuggestionsPage");
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("SelectVegetablePage");
    }
}
