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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLibraryActive))]
    [NotifyPropertyChangedFor(nameof(IsControllersActive))]
    [NotifyPropertyChangedFor(nameof(IsSettingsActive))]
    [NotifyPropertyChangedFor(nameof(IsDiagnosticsActive))]
    [NotifyPropertyChangedFor(nameof(IsAboutActive))]
    private ViewModelBase _currentView;

    [ObservableProperty]
    private string _windowTitle = "PS2 Home";

    public bool IsLibraryActive => CurrentView == _libraryViewModel;
    public bool IsControllersActive => CurrentView == _controllersViewModel;
    public bool IsSettingsActive => CurrentView == _settingsViewModel;
    public bool IsDiagnosticsActive => CurrentView == _diagnosticsViewModel;
    public bool IsAboutActive => CurrentView == _aboutViewModel;

    public MainViewModel(
        LibraryViewModel libraryViewModel,
        SettingsViewModel settingsViewModel,
        DiagnosticsViewModel diagnosticsViewModel,
        ControllersViewModel controllersViewModel,
        AboutViewModel aboutViewModel)
    {
        _libraryViewModel = libraryViewModel;
        _settingsViewModel = settingsViewModel;
        _diagnosticsViewModel = diagnosticsViewModel;
        _controllersViewModel = controllersViewModel;
        _aboutViewModel = aboutViewModel;

        _currentView = libraryViewModel;
        _windowTitle = "PS2 Home - Library";
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
