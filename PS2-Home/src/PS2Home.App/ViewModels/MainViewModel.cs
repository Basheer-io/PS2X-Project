using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PS2Home.App.ViewModels;

/// <summary>
/// Main window ViewModel handling navigation between Library, Settings, Diagnostics, Controllers, and About views.
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly LibraryViewModel _libraryViewModel;
    private readonly SettingsViewModel _settingsViewModel;
    private readonly DiagnosticsViewModel _diagnosticsViewModel;
    private readonly ControllersViewModel _controllersViewModel;
    private readonly AboutViewModel _aboutViewModel;
    private readonly DownloadsViewModel _downloadsViewModel;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLibraryActive))]
    [NotifyPropertyChangedFor(nameof(IsControllersActive))]
    [NotifyPropertyChangedFor(nameof(IsSettingsActive))]
    [NotifyPropertyChangedFor(nameof(IsDiagnosticsActive))]
    [NotifyPropertyChangedFor(nameof(IsAboutActive))]
    [NotifyPropertyChangedFor(nameof(IsDownloadsActive))]
    private ViewModelBase _currentView;

    [ObservableProperty]
    private string _windowTitle = "PS2 Home";

    public bool IsLibraryActive => CurrentView == _libraryViewModel;
    public bool IsControllersActive => CurrentView == _controllersViewModel;
    public bool IsSettingsActive => CurrentView == _settingsViewModel;
    public bool IsDiagnosticsActive => CurrentView == _diagnosticsViewModel;
    public bool IsAboutActive => CurrentView == _aboutViewModel;
    public bool IsDownloadsActive => CurrentView == _downloadsViewModel;

    public MainViewModel(
        LibraryViewModel libraryViewModel,
        SettingsViewModel settingsViewModel,
        DiagnosticsViewModel diagnosticsViewModel,
        ControllersViewModel controllersViewModel,
        AboutViewModel aboutViewModel,
        DownloadsViewModel downloadsViewModel)
    {
        _libraryViewModel = libraryViewModel;
        _settingsViewModel = settingsViewModel;
        _diagnosticsViewModel = diagnosticsViewModel;
        _controllersViewModel = controllersViewModel;
        _aboutViewModel = aboutViewModel;
        _downloadsViewModel = downloadsViewModel;

        _currentView = libraryViewModel;
        _windowTitle = "PS2 Home - Library";
    }

    public async Task OpenGamePathAsync(string gamePath)
    {
        CurrentView = _libraryViewModel;
        WindowTitle = "PS2 Home - Library";
        await _libraryViewModel.LaunchGamePathAsync(gamePath);
    }

    [RelayCommand]
    private void NavigateToDownloads()
    {
        CurrentView = _downloadsViewModel;
        WindowTitle = "PS2 Home - Legal Downloads";
    }

    [RelayCommand]
    private void NavigateToLibrary()
    {
        CurrentView = _libraryViewModel;
        WindowTitle = "PS2 Home - Library";
    }

    [RelayCommand]
    private void NavigateToControllers()
    {
        CurrentView = _controllersViewModel;
        WindowTitle = "PS2 Home - Controllers";
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        CurrentView = _settingsViewModel;
        WindowTitle = "PS2 Home - Settings";
    }

    [RelayCommand]
    private void NavigateToDiagnostics()
    {
        CurrentView = _diagnosticsViewModel;
        WindowTitle = "PS2 Home - Diagnostics";
    }

    [RelayCommand]
    private void NavigateToAbout()
    {
        CurrentView = _aboutViewModel;
        WindowTitle = "PS2 Home - About";
    }
}
