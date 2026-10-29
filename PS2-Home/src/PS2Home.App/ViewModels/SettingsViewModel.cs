using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PS2Home.Core;

namespace PS2Home.App.ViewModels;

/// <summary>
/// Settings view model - manages game folders, PCSX2 detection, setup validation, couch mode, backup/restore, and persistence.
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsStore _settingsStore;
    private readonly IPcsx2Locator _pcsx2Locator;
    private readonly ISettingsBackupService _backupService;
    private readonly IGameLauncher _gameLauncher;

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
    private bool _couchMode = false;

    [ObservableProperty]
    private string _theme = "Dark";

    [ObservableProperty]
    private string _statusMessage = "Settings ready";

    [ObservableProperty]
    private string _backupStatusMessage = "Export or restore configuration and game library.";

    // PCSX2 Setup Assistant
    [ObservableProperty]
    private ObservableCollection<Pcsx2Installation> _detectedInstallations = [];

    [ObservableProperty]
    private Pcsx2Installation? _selectedInstallation;

    [ObservableProperty]
    private string _customPcsx2Path = string.Empty;

    [ObservableProperty]
    private bool _isDetecting = false;

    [ObservableProperty]
    private bool _isValidating = false;

    [ObservableProperty]
    private bool _isBackingUp = false;

    [ObservableProperty]
    private bool _isPcsx2Configured = false;

    [ObservableProperty]
    private string _pcsx2StatusText = "PCSX2 not configured";

    [ObservableProperty]
    private string _pcsx2VersionText = string.Empty;

    public SettingsViewModel() : this(
        new PS2Home.Infrastructure.JsonSettingsStore(),
        new PS2Home.Infrastructure.Pcsx2Locator(),
        new PS2Home.Infrastructure.SettingsBackupService(
            new PS2Home.Infrastructure.JsonSettingsStore(),
            new PS2Home.Infrastructure.JsonLibraryStore()),
        new PS2Home.Infrastructure.GameLauncher(new PS2Home.Infrastructure.LaunchLogger()))
    {
    }

    public SettingsViewModel(
        ISettingsStore settingsStore,
        IPcsx2Locator pcsx2Locator,
        ISettingsBackupService backupService,
        IGameLauncher gameLauncher)
    {
        _settingsStore = settingsStore;
        _pcsx2Locator = pcsx2Locator;
        _backupService = backupService;
        _gameLauncher = gameLauncher;
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
            CouchMode = settings.CouchMode;
            Theme = settings.Theme;

            if (settings.Pcsx2 != null && !string.IsNullOrWhiteSpace(settings.Pcsx2.Command))
            {
                SelectedInstallation = settings.Pcsx2;
                CustomPcsx2Path = settings.Pcsx2.Kind != Pcsx2InstallKind.Flatpak ? settings.Pcsx2.Command : string.Empty;
                IsPcsx2Configured = settings.Pcsx2.IsValid;
                Pcsx2StatusText = settings.Pcsx2.IsValid ? $"Configured: {settings.Pcsx2.DisplayName}" : "Saved PCSX2 path needs verification";
                Pcsx2VersionText = settings.Pcsx2.Version ?? string.Empty;
            }
            else
            {
                // Automatically run discovery on first setup
                _ = DetectPcsx2Async();
            }

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
                CouchMode = CouchMode,
                Theme = Theme,
                Pcsx2 = SelectedInstallation
            };

            await _settingsStore.SaveSettingsAsync(settings);
            StatusMessage = "Settings and PCSX2 configuration saved successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving settings: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task DetectPcsx2Async()
    {
        if (IsDetecting) return;

        IsDetecting = true;
        StatusMessage = "Searching for PCSX2 installations...";

        try
        {
            var found = await _pcsx2Locator.DetectInstallationsAsync();
            DetectedInstallations.Clear();

            foreach (var item in found)
            {
                DetectedInstallations.Add(item);
            }

            if (DetectedInstallations.Count > 0)
            {
                SelectedInstallation = DetectedInstallations[0];
                IsPcsx2Configured = true;
                Pcsx2StatusText = $"Found: {SelectedInstallation.DisplayName}";
                Pcsx2VersionText = SelectedInstallation.Version ?? "Version unknown";
                StatusMessage = $"Detected {DetectedInstallations.Count} PCSX2 installation(s)";
            }
            else
            {
                IsPcsx2Configured = false;
                Pcsx2StatusText = "PCSX2 not detected. You can specify the executable path below.";
                StatusMessage = "No PCSX2 installations detected in common locations";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Detection failed: {ex.Message}";
        }
        finally
        {
            IsDetecting = false;
        }
    }

    [RelayCommand]
    public async Task ValidateCustomPathAsync()
    {
        if (string.IsNullOrWhiteSpace(CustomPcsx2Path))
        {
            StatusMessage = "Please enter an executable path to validate";
            return;
        }

        [RelayCommand]
        public async Task OpenPcsx2Async()
        {
            if (SelectedInstallation == null || !SelectedInstallation.IsValid)
            {
                StatusMessage = "Select or validate a PCSX2 installation first";
                return;
            }

            try
            {
                await _gameLauncher.LaunchEmulatorAsync(SelectedInstallation);
                StatusMessage = "PCSX2 opened successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Could not open PCSX2: {ex.Message}";
            }
        }

        IsValidating = true;
        StatusMessage = "Validating binary...";

        try
        {
            var kind = CustomPcsx2Path.EndsWith(".AppImage", StringComparison.OrdinalIgnoreCase)
                ? Pcsx2InstallKind.AppImage
                : Pcsx2InstallKind.Executable;

            var install = Pcsx2Installation.CreateExecutable(CustomPcsx2Path.Trim(), kind);
            var validation = await _pcsx2Locator.ValidateInstallationAsync(install);

            if (validation.IsValid)
            {
                SelectedInstallation = install with { Version = validation.Version, IsValid = true };
                IsPcsx2Configured = true;
                Pcsx2StatusText = $"Valid: {SelectedInstallation.DisplayName}";
                Pcsx2VersionText = validation.Version ?? "Valid";
                StatusMessage = "PCSX2 executable validated successfully";
            }
            else
            {
                IsPcsx2Configured = false;
                Pcsx2StatusText = $"Invalid: {validation.ErrorMessage}";
                StatusMessage = validation.ErrorMessage ?? "Validation failed";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Validation error: {ex.Message}";
        }
        finally
        {
            IsValidating = false;
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
        CouchMode = false;
        Theme = "Dark";
        StatusMessage = "Settings reset to defaults (unsaved)";
    }

    public async Task ExportBackupToPathAsync(string targetFilePath)
    {
        IsBackingUp = true;
        BackupStatusMessage = "Exporting backup...";

        try
        {
            var savedPath = await _backupService.ExportBackupAsync(targetFilePath);
            BackupStatusMessage = $"Backup exported successfully to: {Path.GetFileName(savedPath)}";
        }
        catch (Exception ex)
        {
            BackupStatusMessage = $"Export failed: {ex.Message}";
        }
        finally
        {
            IsBackingUp = false;
        }
    }

    public async Task<bool> RestoreBackupFromPathAsync(string sourceFilePath)
    {
        IsBackingUp = true;
        BackupStatusMessage = "Validating and restoring backup...";

        try
        {
            var validation = await _backupService.ValidateBackupAsync(sourceFilePath);
            if (!validation.IsValid)
            {
                BackupStatusMessage = $"Import validation failed: {validation.ErrorMessage}";
                return false;
            }

            var success = await _backupService.RestoreBackupAsync(sourceFilePath);
            if (success)
            {
                await LoadSettingsAsync();
                BackupStatusMessage = $"Backup restored successfully ({validation.Data?.Games.Count ?? 0} games, {validation.Data?.Settings.GameFolders.Count ?? 0} folders)";
                return true;
            }

            BackupStatusMessage = "Restore failed.";
            return false;
        }
        catch (Exception ex)
        {
            BackupStatusMessage = $"Restore failed: {ex.Message}";
            return false;
        }
        finally
        {
            IsBackingUp = false;
        }
    }
}
