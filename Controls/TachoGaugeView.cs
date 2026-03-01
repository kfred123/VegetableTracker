namespace VegetableTracker.Controls;

/// <summary>
/// A tachometer-style gauge view that wraps <see cref="TachoGaugeDrawable"/>
/// inside a <see cref="GraphicsView"/>.
/// </summary>
public class TachoGaugeView : ContentView
{
    private readonly TachoGaugeDrawable _drawable;
    private readonly GraphicsView _graphicsView;

    // ── Bindable Properties ──

    public static readonly BindableProperty ProgressProperty =
        BindableProperty.Create(nameof(Progress), typeof(double), typeof(TachoGaugeView), 0.0,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty CurrentValueProperty =
        BindableProperty.Create(nameof(CurrentValue), typeof(int), typeof(TachoGaugeView), 0,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty MaxValueProperty =
        BindableProperty.Create(nameof(MaxValue), typeof(int), typeof(TachoGaugeView), 30,
            propertyChanged: OnVisualPropertyChanged);

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public int CurrentValue
    {
        get => (int)GetValue(CurrentValueProperty);
        set => SetValue(CurrentValueProperty, value);
    }

    public int MaxValue
    {
        get => (int)GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    public TachoGaugeView()
    {
        _drawable = new TachoGaugeDrawable();

        _graphicsView = new GraphicsView
        {
            Drawable = _drawable,
            HeightRequest = 240,
            WidthRequest = 350,
            HorizontalOptions = LayoutOptions.Center,
            BackgroundColor = Colors.Transparent,
        };

        Content = _graphicsView;
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TachoGaugeView view)
        {
            view._drawable.Progress = view.Progress;
            view._drawable.CurrentValue = view.CurrentValue;
            view._drawable.MaxValue = view.MaxValue;
            view._graphicsView.Invalidate();
        }
    }
}
