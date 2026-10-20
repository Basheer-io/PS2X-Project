using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PS2Home.App.ViewModels;

/// <summary>
/// Settings view model - manages application configuration.
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _ps2AppsPath = string.Empty;

    [ObservableProperty]
    private string _coverArtPath = string.Empty;

    [ObservableProperty]
    private bool _autoScanOnStartup = true;

    [ObservableProperty]
    private bool _showHiddenApps = false;

    [ObservableProperty]
    private string _theme = "Dark";

    [ObservableProperty]
    private string _statusMessage = "Settings loaded";

    public SettingsViewModel()
    {
        LoadSettings();
    }

    [RelayCommand]
    private void SaveSettings()
    {
        // TODO: Persist settings to disk
        StatusMessage = "Settings saved successfully";
    }

    [RelayCommand]
    private void ResetToDefaults()
    {
        Ps2AppsPath = string.Empty;
        CoverArtPath = string.Empty;
        AutoScanOnStartup = true;
        ShowHiddenApps = false;
        Theme = "Dark";
        StatusMessage = "Settings reset to defaults";
    }

    [RelayCommand]
    private void BrowsePs2AppsPath()
    {
        // TODO: Open folder picker dialog
        StatusMessage = "Folder picker not implemented yet";
    }

    [RelayCommand]
    private void BrowseCoverArtPath()
    {
        // TODO: Open folder picker dialog
        StatusMessage = "Folder picker not implemented yet";
    }

    private void LoadSettings()
    {
        // TODO: Load from persistent storage
        Ps2AppsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/PS2Apps";
        CoverArtPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/PS2Covers";
        StatusMessage = "Settings loaded from defaults";
    }
}