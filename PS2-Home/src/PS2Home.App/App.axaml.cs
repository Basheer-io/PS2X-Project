using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using PS2Home.App.ViewModels;
using PS2Home.Core;
using PS2Home.Infrastructure;

namespace PS2Home.App;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();

        // Core & Infrastructure services
        services.AddSingleton<ISettingsStore, JsonSettingsStore>();
        services.AddSingleton<ILibraryStore, JsonLibraryStore>();
        services.AddSingleton<IGameScanner, FileSystemGameScanner>();
        services.AddSingleton<IGameLibrary, RealGameLibrary>();

        // ViewModels
        services.AddSingleton<LibraryViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<DiagnosticsViewModel>();
        services.AddSingleton<MainViewModel>();

        // Set up DI container
        Ioc.Default.ConfigureServices(services.BuildServiceProvider());

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainViewModel = Ioc.Default.GetRequiredService<MainViewModel>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
