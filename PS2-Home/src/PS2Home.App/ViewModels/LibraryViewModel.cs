using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PS2Home.Core;
using System.Collections.ObjectModel;

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
    private string _coverArtPath = string.Empty;

    [ObservableProperty]
    private bool _isInstalled = true;

    [ObservableProperty]
    private bool _isMissing = false;

    [ObservableProperty]
    private long _fileSizeBytes = 0;

    [ObservableProperty]
    private string _status = "Ready";

    public string FormattedSize => FileSizeBytes > 0
        ? $"{(double)FileSizeBytes / (1024 * 1024 * 1024):0.0} GB"
        : string.Empty;

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
        Status = entry.IsMissing ? "Missing file" : "Ready"
    };

    public static AppItemViewModel FromGameInfo(GameInfo info) => new()
    {
        Id = info.ConfigFolderName,
        Name = info.Name,
        ConfigFolderName = info.ConfigFolderName,
        Tagline = info.Tagline,
        CoverArtPath = info.CoverArtPath,
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
        LastScannedAt = DateTimeOffset.UtcNow
    };

    public GameInfo ToGameInfo() => new(
        Name,
        ConfigFolderName,
        Tagline,
        CoverArtPath,
        IsInstalled,
        Status);
}

/// <summary>
/// Library view model - manages game scanning, progress reporting, cancellation, and search filtering.
/// </summary>
public partial class LibraryViewModel : ViewModelBase
{
    private readonly IGameLibrary _gameLibrary;
    private readonly ISettingsStore _settingsStore;
    private readonly List<AppItemViewModel> _allApps = [];
    private CancellationTokenSource? _scanCts;

    [ObservableProperty]
    private ObservableCollection<AppItemViewModel> _apps = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private bool _isScanning = false;

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

    public LibraryViewModel() : this(new SampleGameLibrary(), new PS2Home.Infrastructure.JsonSettingsStore())
    {
    }

    public LibraryViewModel(IGameLibrary gameLibrary, ISettingsStore settingsStore)
    {
        _gameLibrary = gameLibrary;
        _settingsStore = settingsStore;
        _ = LoadInitialLibraryAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(HasSearchQuery));
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

    public async Task LoadInitialLibraryAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        StatusMessage = "Loading library...";

        try
        {
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
                _allApps.AddRange(entries.Select(AppItemViewModel.FromGameEntry));
            }

            UpdateCounts();
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
            _allApps.AddRange(scannedEntries.Select(AppItemViewModel.FromGameEntry));

            UpdateCounts();
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
    private void LaunchApp(AppItemViewModel? app)
    {
        if (app == null) return;
        if (app.IsMissing)
        {
            StatusMessage = $"Cannot launch {app.Name}: file does not exist at {app.FullPath}";
            return;
        }
        StatusMessage = $"Launching {app.Name}... (Phase 4)";
    }

    private void UpdateCounts()
    {
        TotalApps = _allApps.Count;
        InstalledApps = _allApps.Count(a => !a.IsMissing);
        MissingApps = _allApps.Count(a => a.IsMissing);
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

        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(HasApps));
    }
}
