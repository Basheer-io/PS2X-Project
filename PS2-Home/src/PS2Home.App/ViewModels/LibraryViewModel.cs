using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace PS2Home.App.ViewModels;

/// <summary>
/// Represents a PS2 application/game in the library.
/// </summary>
public partial class AppItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _configFolderName = string.Empty;

    [ObservableProperty]
    private string _tagline = string.Empty;

    [ObservableProperty]
    private string _coverArtPath = string.Empty;

    [ObservableProperty]
    private bool _isInstalled = false;

    [ObservableProperty]
    private string _status = "Not scanned";
}

/// <summary>
/// Library view model - displays installed PS2 applications/games as cards.
/// </summary>
public partial class LibraryViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<AppItemViewModel> _apps = [];

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private int _totalApps = 0;

    [ObservableProperty]
    private int _installedApps = 0;

    public LibraryViewModel()
    {
        LoadSampleData();
    }

    [RelayCommand]
    private async Task RefreshLibraryAsync()
    {
        IsLoading = true;
        StatusMessage = "Scanning for PS2 applications...";
        
        // Simulate scanning delay
        await Task.Delay(1000);
        
        LoadSampleData();
        
        IsLoading = false;
        StatusMessage = $"Found {TotalApps} applications ({InstalledApps} installed)";
    }

    [RelayCommand]
    private void LaunchApp(AppItemViewModel? app)
    {
        if (app == null) return;
        StatusMessage = $"Launching {app.Name}...";
        // TODO: Implement actual launch via Infrastructure layer
    }

    private void LoadSampleData()
    {
        Apps.Clear();
        
        // Sample PS2 homebrew applications
        var sampleApps = new[]
        {
            new AppItemViewModel 
            { 
                Name = "Open PS2 Loader (OPL)", 
                ConfigFolderName = "OPL", 
                Tagline = "Play games from HDD, SMB, or USB",
                CoverArtPath = string.Empty,
                IsInstalled = true,
                Status = "Installed"
            },
            new AppItemViewModel 
            { 
                Name = "FreeMCBoot Installer", 
                ConfigFolderName = "FMCB", 
                Tagline = "Softmod installer for PS2",
                CoverArtPath = string.Empty,
                IsInstalled = true,
                Status = "Installed"
            },
            new AppItemViewModel 
            { 
                Name = "wLaunchELF", 
                ConfigFolderName = "WLELF", 
                Tagline = "File manager and ELF launcher",
                CoverArtPath = string.Empty,
                IsInstalled = false,
                Status = "Not installed"
            },
            new AppItemViewModel 
            { 
                Name = "Simple Media System (SMS)", 
                ConfigFolderName = "SMS", 
                Tagline = "Media player for PS2",
                CoverArtPath = string.Empty,
                IsInstalled = false,
                Status = "Not installed"
            },
            new AppItemViewModel 
            { 
                Name = "PS2 Browser", 
                ConfigFolderName = "PSBROWSER", 
                Tagline = "Homebrew web browser",
                CoverArtPath = string.Empty,
                IsInstalled = false,
                Status = "Not installed"
            },
            new AppItemViewModel 
            { 
                Name = "Cheat Code Manager (CCAPI)", 
                ConfigFolderName = "CCAPI", 
                Tagline = "Cheat code management tool",
                CoverArtPath = string.Empty,
                IsInstalled = false,
                Status = "Not installed"
            }
        };

        foreach (var app in sampleApps)
        {
            Apps.Add(app);
        }

        TotalApps = Apps.Count;
        InstalledApps = Apps.Count(a => a.IsInstalled);
        StatusMessage = $"Found {TotalApps} applications ({InstalledApps} installed)";
    }
}