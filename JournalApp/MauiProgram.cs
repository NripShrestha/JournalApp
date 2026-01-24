using JournalApp.Data;
using JournalApp.Services; // Add this
using Microsoft.Extensions.Logging;

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
            builder.Services.AddSingleton<SecurityService>();

            // Add PDF Export Service
            builder.Services.AddSingleton<PdfExportService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif
            string dbPath = Path.Combine(
                FileSystem.AppDataDirectory,
                "journal.db"
            );
            System.Diagnostics.Debug.WriteLine($"[DB PATH] {dbPath}");

            builder.Services.AddSingleton<AppDatabase>(
                s => new AppDatabase(dbPath)
            );
            builder.Services.AddSingleton<MainPage>();

            return builder.Build();
        }
    }
}