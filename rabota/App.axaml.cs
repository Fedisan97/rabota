using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using rabota.Services;
using rabota.Models;

namespace rabota;

public partial class App : Application
{
    public static DatabaseService? DB { get; private set; }
    public static User? CurrentUser { get; set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        string connectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=Vaser314";
        DB = new DatabaseService(connectionString);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new LoginWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}