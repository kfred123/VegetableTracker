using Microsoft.Extensions.Logging;
using VegetableTracker.Services;

namespace VegetableTracker
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Services
            builder.Services.AddSingleton<DatabaseService>();

            // Pages
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<SelectVegetablePage>();
            builder.Services.AddTransient<DetailsPage>();
            builder.Services.AddTransient<SuggestionsPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
