using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PS2Home.Core;
using System.Collections.ObjectModel;

namespace PS2Home.App.ViewModels;

/// <summary>
/// Settings view model - manages application configuration and persistent storage.
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsStore _settingsStore;

    [ObservableProperty]
    private ObservableCollection<string> _gameFolders = [];

    [ObservableProperty]
    private string _newFolderPath = string.Empty;

    [ObservableProperty]
    private string _selectedFolder = string.Empty;

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

    public SettingsViewModel() : this(new PS2Home.Infrastructure.JsonSettingsStore())
    {
    }

    public SettingsViewModel(ISettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
        _ = LoadSettingsAsync();
    }

    [RelayCommand]
    public async Task LoadSettingsAsync()
    {
        try
        {
            var settings = await _settingsStore.LoadSettingsAsync();
            GameFolders.Clear();
            foreach (var folder in settings.GameFolders)
            {
                GameFolders.Add(folder);
            }

            CoverArtPath = settings.CoverArtFolder;
            AutoScanOnStartup = settings.AutoScanOnStartup;
            ShowHiddenApps = settings.ShowHiddenApps;
            Theme = settings.Theme;
            StatusMessage = "Settings loaded";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load settings: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task SaveSettingsAsync()
    {
        try
        {
            var settings = new AppSettings
            {
                GameFolders = [.. GameFolders],
                CoverArtFolder = CoverArtPath,
                AutoScanOnStartup = AutoScanOnStartup,
                ShowHiddenApps = ShowHiddenApps,
                Theme = Theme
            };

            await _settingsStore.SaveSettingsAsync(settings);
            StatusMessage = "Settings saved successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving settings: {ex.Message}";
        }
    }

    [RelayCommand]
    public void AddFolder()
    {
        if (string.IsNullOrWhiteSpace(NewFolderPath))
            return;

        var normalized = NewFolderPath.Trim();
        if (!GameFolders.Contains(normalized))
        {
            GameFolders.Add(normalized);
            NewFolderPath = string.Empty;
            StatusMessage = $"Added folder: {normalized}";
        }
        else
        {
            StatusMessage = "Folder is already in the list";
        }
    }

    [RelayCommand]
    public void RemoveFolder(string? folder)
    {
        var target = folder ?? SelectedFolder;
        if (!string.IsNullOrWhiteSpace(target) && GameFolders.Remove(target))
        {
            StatusMessage = $"Removed folder: {target}";
        }
    }

    [RelayCommand]
    public void ResetToDefaults()
    {
        GameFolders.Clear();
        var defaultFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "PS2Games");
        GameFolders.Add(defaultFolder);

        CoverArtPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "PS2Covers");
        AutoScanOnStartup = true;
        ShowHiddenApps = false;
        Theme = "Dark";
        StatusMessage = "Settings reset to defaults (unsaved)";
    }
}
