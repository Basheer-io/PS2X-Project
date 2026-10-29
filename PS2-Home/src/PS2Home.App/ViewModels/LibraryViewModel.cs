using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PS2Home.Core;
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

    public static AppItemViewModel FromGameInfo(GameInfo info) => new()
    {
        Name = info.Name,
        ConfigFolderName = info.ConfigFolderName,
        Tagline = info.Tagline,
        CoverArtPath = info.CoverArtPath,
        IsInstalled = info.IsInstalled,
        Status = info.Status
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
/// Library view model - displays installed PS2 applications/games as cards with filtering.
/// </summary>
public partial class LibraryViewModel : ViewModelBase
{
    private readonly IGameLibrary _gameLibrary;
    private readonly List<AppItemViewModel> _allApps = [];

    [ObservableProperty]
    private ObservableCollection<AppItemViewModel> _apps = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private int _totalApps = 0;

    [ObservableProperty]
    private int _installedApps = 0;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool IsEmpty => !IsLoading && !HasError && Apps.Count == 0;
    public bool HasApps => !IsLoading && !HasError && Apps.Count > 0;
    public bool HasSearchQuery => !string.IsNullOrWhiteSpace(SearchText);

    public LibraryViewModel() : this(new SampleGameLibrary())
    {
    }

    public LibraryViewModel(IGameLibrary gameLibrary)
    {
        _gameLibrary = gameLibrary;
        _ = RefreshLibraryAsync();
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

    partial void OnErrorMessageChanged(string? value)
    {
        OnPropertyChanged(nameof(HasError));
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(HasApps));
    }

    [RelayCommand]
    public async Task RefreshLibraryAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        StatusMessage = "Scanning for PS2 applications...";

        try
        {
            // Small simulated delay for responsive UI demonstration
            await Task.Delay(300);

            var games = await _gameLibrary.GetAllGamesAsync();
            _allApps.Clear();
            _allApps.AddRange(games.Select(AppItemViewModel.FromGameInfo));

            TotalApps = _allApps.Count;
            InstalledApps = _allApps.Count(a => a.IsInstalled);

            ApplyFilter();
            StatusMessage = $"Found {TotalApps} applications ({InstalledApps} installed)";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load library: {ex.Message}";
            StatusMessage = "Error scanning library";
        }
        finally
        {
            IsLoading = false;
        }
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
        StatusMessage = $"Launching {app.Name}...";
    }

    private void ApplyFilter()
    {
        var gameInfos = _allApps.Select(a => a.ToGameInfo());
        var filteredInfos = GameFilter.Apply(gameInfos, SearchText);
        var filteredSet = new HashSet<string>(filteredInfos.Select(g => g.Name));

        Apps.Clear();
        foreach (var app in _allApps.Where(a => filteredSet.Contains(a.Name)))
        {
            Apps.Add(app);
        }

        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(HasApps));
    }
}
