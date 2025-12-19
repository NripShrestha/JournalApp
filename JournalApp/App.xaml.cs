namespace JournalApp
{
    public partial class App : Application
    {
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(
                Handler.MauiContext!.Services.GetRequiredService<MainPage>()
            );
        }
    }
}
