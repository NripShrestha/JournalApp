using Microsoft.Extensions.Logging;
using JournalApp.Data;

namespace JournalApp
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
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            string dbPath = Path.Combine(
                FileSystem.AppDataDirectory,
                "journal.db"
            );

            builder.Services.AddSingleton<AppDatabase>(
                s => new AppDatabase(dbPath)
            );
            builder.Services.AddSingleton<MainPage>();

            return builder.Build();
        }
    }
}
