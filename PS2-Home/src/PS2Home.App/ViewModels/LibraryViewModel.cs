using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PS2Home.Core;

namespace PS2Home.App.ViewModels;

/// <summary>
/// Represents a PS2 game or application card in the library.
/// </summary>
public partial class AppItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _fullPath = string.Empty;

    [ObservableProperty]
    private string _configFolderName = string.Empty;

    [ObservableProperty]
    private string _tagline = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCoverArt))]
    private string? _coverArtPath;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedLastPlayed))]
    private DateTimeOffset? _lastPlayedAt;

    [ObservableProperty]
    private bool _isInstalled = true;

    [ObservableProperty]
    private bool _isMissing = false;

    [ObservableProperty]
    private bool _isRunning = false;

    [ObservableProperty]
    private bool _isLaunching = false;

    [ObservableProperty]
    private long _fileSizeBytes = 0;

    [ObservableProperty]
    private string _status = "Ready";

    public bool HasCoverArt => !string.IsNullOrWhiteSpace(CoverArtPath) && File.Exists(CoverArtPath);

    public string FormattedSize => FileSizeBytes > 0
        ? $"{(double)FileSizeBytes / (1024 * 1024 * 1024):0.0} GB"
        : string.Empty;

    public string FormattedLastPlayed => LastPlayedAt.HasValue
        ? $"Played {FormatRelativeTime(LastPlayedAt.Value)}"
        : "Never played";

    public string LaunchButtonText
    {
        get
        {
            if (IsRunning) return "🎮 Running";
            if (IsLaunching) return "⏳ Starting...";
            if (IsMissing) return "Unavailable";
            return "▶ Launch";
        }
    }

    public bool CanLaunch => !IsMissing && !IsRunning && !IsLaunching;

    partial void OnIsRunningChanged(bool value)
    {
        OnPropertyChanged(nameof(LaunchButtonText));
        OnPropertyChanged(nameof(CanLaunch));
    }

    partial void OnIsLaunchingChanged(bool value)
    {
        OnPropertyChanged(nameof(LaunchButtonText));
        OnPropertyChanged(nameof(CanLaunch));
    }

    partial void OnIsMissingChanged(bool value)
    {
        OnPropertyChanged(nameof(LaunchButtonText));
        OnPropertyChanged(nameof(CanLaunch));
    }

    public static string FormatRelativeTime(DateTimeOffset dt)
    {
        var span = DateTimeOffset.UtcNow - dt;
        if (span.TotalMinutes < 1) return "Just now";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
        if (span.TotalDays < 7) return $"{(int)span.TotalDays}d ago";
        return dt.ToString("yyyy-MM-dd");
    }

    public static AppItemViewModel FromGameEntry(GameEntry entry) => new()
    {
        Id = entry.Id,
        Name = entry.Title,
        FullPath = entry.FullPath,
        ConfigFolderName = Path.GetFileName(entry.FullPath),
        Tagline = entry.FullPath,
        FileSizeBytes = entry.FileSizeBytes,
        IsInstalled = !entry.IsMissing,
        IsMissing = entry.IsMissing,
        Status = entry.IsMissing ? "Missing file" : "Ready",
        LastPlayedAt = entry.LastPlayedAt,
        CoverArtPath = entry.CoverArtPath
    };

    public static AppItemViewModel FromGameInfo(GameInfo info) => new()
    {
        Id = info.ConfigFolderName,
        Name = info.Name,
        ConfigFolderName = info.ConfigFolderName,
        Tagline = info.Tagline,
        CoverArtPath = string.IsNullOrWhiteSpace(info.CoverArtPath) ? null : info.CoverArtPath,
        IsInstalled = info.IsInstalled,
        IsMissing = !info.IsInstalled,
        Status = info.Status
    };

    public GameEntry ToGameEntry() => new()
    {
        Id = Id,
        Title = Name,
        FullPath = FullPath,
        FileSizeBytes = FileSizeBytes,
        IsMissing = IsMissing,
        AddedAt = DateTimeOffset.UtcNow,
        LastScannedAt = DateTimeOffset.UtcNow,
        LastPlayedAt = LastPlayedAt,
        CoverArtPath = CoverArtPath
    };

    public GameInfo ToGameInfo() => new(
        Name,
        ConfigFolderName,
        Tagline,
        CoverArtPath ?? string.Empty,
        IsInstalled,
        Status);
}

/// <summary>
/// Library view model - manages game scanning, progress reporting, recents ordering, cover art, and process launching.
/// </summary>
public partial class LibraryViewModel : ViewModelBase
{
    private readonly IGameLibrary _gameLibrary;
    private readonly ISettingsStore _settingsStore;
    private readonly IGameLauncher _gameLauncher;
    private readonly ILaunchLogger _launchLogger;
    private readonly ICoverArtManager _coverArtManager;
    private readonly List<AppItemViewModel> _allApps = [];
    private CancellationTokenSource? _scanCts;
    private IGameSession? _activeSession;
    private AppItemViewModel? _activeGame;

    [ObservableProperty]
    private ObservableCollection<AppItemViewModel> _apps = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasRecentApps))]
    private ObservableCollection<AppItemViewModel> _recentApps = [];

    [ObservableProperty]
    private AppItemViewModel? _selectedApp;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private bool _isScanning = false;

    [ObservableProperty]
    private bool _couchMode = false;

    [ObservableProperty]
    private string _scanProgressMessage = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private int _totalApps = 0;

    [ObservableProperty]
    private int _installedApps = 0;

    [ObservableProperty]
    private int _missingApps = 0;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool IsEmpty => !IsLoading && !IsScanning && !HasError && Apps.Count == 0;
    public bool HasApps => !IsLoading && !IsScanning && !HasError && Apps.Count > 0;
    public bool HasSearchQuery => !string.IsNullOrWhiteSpace(SearchText);
    public bool HasRecentApps => RecentApps.Count > 0 && !HasSearchQuery;

    public LibraryViewModel() : this(
        new SampleGameLibrary(),
        new PS2Home.Infrastructure.JsonSettingsStore(),
        new PS2Home.Infrastructure.GameLauncher(new PS2Home.Infrastructure.LaunchLogger()),
        new PS2Home.Infrastructure.LaunchLogger(),
        new PS2Home.Infrastructure.CoverArtManager())
    {
    }

    public LibraryViewModel(
        IGameLibrary gameLibrary,
        ISettingsStore settingsStore,
        IGameLauncher gameLauncher,
        ILaunchLogger launchLogger,
        ICoverArtManager coverArtManager)
    {
        _gameLibrary = gameLibrary;
        _settingsStore = settingsStore;
        _gameLauncher = gameLauncher;
        _launchLogger = launchLogger;
        _coverArtManager = coverArtManager;
        _ = LoadInitialLibraryAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(HasSearchQuery));
        OnPropertyChanged(nameof(HasRecentApps));
        ApplyFilter();
    }

    partial void OnIsLoadingChanged(bool value)
    {
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(HasApps));
    }

    partial void OnIsScanningChanged(bool value)
    {
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(HasApps));
    }

    partial void OnErrorMessageChanged(string? value)
    {
        OnPropertyChanged(nameof(HasError));
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(HasApps));
    }

    partial void OnCouchModeChanged(bool value)
    {
        _ = SaveCouchModeAsync(value);
    }

    private async Task SaveCouchModeAsync(bool value)
    {
        try
        {
            var settings = await _settingsStore.LoadSettingsAsync();
            if (settings.CouchMode != value)
            {
                await _settingsStore.SaveSettingsAsync(settings with { CouchMode = value });
            }
        }
        catch
        {
            // Ignore settings save failure on toggle
        }
    }

    public async Task LoadInitialLibraryAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        StatusMessage = "Loading library...";

        try
        {
            var settings = await _settingsStore.LoadSettingsAsync();
            CouchMode = settings.CouchMode;

            var entries = await _gameLibrary.GetGamesAsync();
            if (entries.Count == 0)
            {
                // Fallback to sample data if library is completely empty
                var legacy = await _gameLibrary.GetAllGamesAsync();
                _allApps.Clear();
                _allApps.AddRange(legacy.Select(AppItemViewModel.FromGameInfo));
            }
            else
            {
                _allApps.Clear();
                _allApps.AddRange(entries.Select(e =>
                {
                    var resolvedCover = _coverArtManager.ResolveCoverArtPath(e.Id, e.CoverArtPath);
                    return AppItemViewModel.FromGameEntry(e with { CoverArtPath = resolvedCover });
                }));
            }

            UpdateCounts();
            UpdateRecentApps();
            ApplyFilter();
            StatusMessage = $"Loaded {TotalApps} items ({MissingApps} missing)";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load library: {ex.Message}";
            StatusMessage = "Error loading library";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task RescanLibraryAsync()
    {
        if (IsScanning) return;

        IsScanning = true;
        ErrorMessage = null;
        ScanProgressMessage = "Preparing to scan folders...";
        StatusMessage = "Scanning for game images...";

        _scanCts = new CancellationTokenSource();

        try
        {
            var settings = await _settingsStore.LoadSettingsAsync(_scanCts.Token);
            var folders = settings.GameFolders.Count > 0
                ? settings.GameFolders
                : [Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "PS2Games")];

            var progress = new Progress<ScanProgress>(p =>
            {
                ScanProgressMessage = $"Scanned {p.FoldersScanned} folders, found {p.GamesFound} games...";
            });

            var scannedEntries = await _gameLibrary.RescanAsync(folders, progress, _scanCts.Token);

            _allApps.Clear();
            _allApps.AddRange(scannedEntries.Select(e =>
            {
                var resolvedCover = _coverArtManager.ResolveCoverArtPath(e.Id, e.CoverArtPath);
                return AppItemViewModel.FromGameEntry(e with { CoverArtPath = resolvedCover });
            }));

            UpdateCounts();
            UpdateRecentApps();
            ApplyFilter();

            StatusMessage = $"Scan completed: {TotalApps} games found ({MissingApps} missing)";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Scan was cancelled";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Scan failed: {ex.Message}";
            StatusMessage = "Scan encountered an error";
        }
        finally
        {
            IsScanning = false;
            ScanProgressMessage = string.Empty;
            _scanCts?.Dispose();
            _scanCts = null;
        }
    }

    [RelayCommand]
    public void CancelScan()
    {
        _scanCts?.Cancel();
    }

    [RelayCommand]
    public void ClearSearch()
    {
        SearchText = string.Empty;
    }

    [RelayCommand]
    public void ToggleCouchMode()
    {
        CouchMode = !CouchMode;
    }

    [RelayCommand]
    public void ToggleErrorState()
    {
        if (HasError)
        {
            ErrorMessage = null;
        }
        else
        {
            ErrorMessage = "Simulated library error for UI testing.";
        }
    }

    [RelayCommand]
    public async Task LaunchAppAsync(AppItemViewModel? app)
    {
        if (app == null) return;
        if (app.IsMissing)
        {
            StatusMessage = $"Cannot launch {app.Name}: file does not exist at {app.FullPath}";
            return;
        }

        if (_activeSession != null && _activeSession.State == GameLaunchState.Running)
        {
            StatusMessage = $"Another game is already running ({_activeGame?.Name ?? "Session"}). Close it before launching another.";
            return;
        }

        var settings = await _settingsStore.LoadSettingsAsync();
        var pcsx2 = settings.Pcsx2;
        if (pcsx2 == null || string.IsNullOrWhiteSpace(pcsx2.Command))
        {
            ErrorMessage = "PCSX2 is not configured. Please select or detect your PCSX2 installation in Settings.";
            StatusMessage = "PCSX2 not configured";
            return;
        }

        app.IsLaunching = true;
        StatusMessage = $"Starting {app.Name}...";

        try
        {
            var request = new LaunchRequest
            {
                Installation = pcsx2,
                GamePath = app.FullPath,
                Fullscreen = true,
                BatchMode = true,
                NoGui = false
            };

            var session = await _gameLauncher.LaunchGameAsync(request);
            _activeSession = session;
            _activeGame = app;

            app.IsLaunching = false;
            app.IsRunning = true;
            app.LastPlayedAt = DateTimeOffset.UtcNow;
            StatusMessage = $"Playing: {app.Name}";

            // Persist last played timestamp
            _ = _gameLibrary.UpdateGamePlayedAsync(app.Id, app.LastPlayedAt.Value);
            UpdateRecentApps();

            // Monitor exit asynchronously without blocking UI
            _ = HandleSessionExitAsync(app, session);
        }
        catch (Exception ex)
        {
            app.IsLaunching = false;
            app.IsRunning = false;
            ErrorMessage = $"Failed to launch {app.Name}: {ex.Message}";
            StatusMessage = "Launch failed";
        }
    }

    [RelayCommand]
    public async Task PlaySelectedAppAsync()
    {
        if (SelectedApp != null && SelectedApp.CanLaunch)
        {
            await LaunchAppAsync(SelectedApp);
        }
    }

    public async Task SetCoverArtAsync(AppItemViewModel app, string imagePath)
    {
        try
        {
            var newPath = await _coverArtManager.SetCoverArtAsync(app.Id, imagePath);
            app.CoverArtPath = newPath;
            await _gameLibrary.UpdateCoverArtAsync(app.Id, newPath);
            StatusMessage = $"Cover art updated for {app.Name}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to set cover art: {ex.Message}";
        }
    }

    public async Task RemoveCoverArtAsync(AppItemViewModel app)
    {
        try
        {
            await _coverArtManager.RemoveCoverArtAsync(app.Id);
            app.CoverArtPath = null;
            await _gameLibrary.UpdateCoverArtAsync(app.Id, null);
            StatusMessage = $"Cover art removed for {app.Name}";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to remove cover art: {ex.Message}";
        }
    }

    private async Task HandleSessionExitAsync(AppItemViewModel app, IGameSession session)
    {
        try
        {
            var result = await session.WaitForExitAsync();
            app.IsRunning = false;

            if (_activeGame == app)
            {
                _activeGame = null;
                _activeSession = null;
            }

            if (result.FastFailure)
            {
                ErrorMessage = $"'{app.Name}' closed immediately (Exit code: {result.ExitCode}). " +
                               (!string.IsNullOrWhiteSpace(result.ErrorSummary) ? $"\nDetails: {result.ErrorSummary}" : "");
                StatusMessage = $"Process exited quickly ({result.ExitCode})";
            }
            else
            {
                StatusMessage = $"Finished playing {app.Name} ({result.Duration.TotalMinutes:0.0} min)";
            }
        }
        catch (Exception ex)
        {
            app.IsRunning = false;
            StatusMessage = $"Session ended: {ex.Message}";
        }
    }

    private void UpdateCounts()
    {
        TotalApps = _allApps.Count;
        InstalledApps = _allApps.Count(a => !a.IsMissing);
        MissingApps = _allApps.Count(a => a.IsMissing);
    }

    public void UpdateRecentApps()
    {
        var entries = _allApps.Select(a => a.ToGameEntry());
        var recentEntries = GameFilter.GetRecentlyPlayed(entries, 5);

        RecentApps.Clear();
        foreach (var entry in recentEntries)
        {
            var match = _allApps.FirstOrDefault(a => a.Id == entry.Id);
            if (match != null)
            {
                RecentApps.Add(match);
            }
        }

        OnPropertyChanged(nameof(HasRecentApps));
    }

    private void ApplyFilter()
    {
        var entries = _allApps.Select(a => a.ToGameEntry());
        var filtered = GameFilter.Apply(entries, SearchText);
        var filteredIds = new HashSet<string>(filtered.Select(g => g.Id));

        Apps.Clear();
        foreach (var app in _allApps.Where(a => filteredIds.Contains(a.Id)))
        {
            Apps.Add(app);
        }

        // Set selected app to first if current selection is invalid
        if (SelectedApp == null || !Apps.Contains(SelectedApp))
        {
            SelectedApp = Apps.FirstOrDefault();
        }

        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(HasApps));
    }
}
