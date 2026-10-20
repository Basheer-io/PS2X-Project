using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PS2Home.App.ViewModels;

/// <summary>
/// Main window ViewModel handling navigation between Library, Settings, and Diagnostics views.
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentView;

    [ObservableProperty]
    private string _windowTitle = "PS2 Home";

    public MainViewModel(
        LibraryViewModel libraryViewModel,
        SettingsViewModel settingsViewModel,
        DiagnosticsViewModel diagnosticsViewModel)
    {
        // Initialize with Library as default view
        _currentView = libraryViewModel;
        
        // Store references for navigation
        _libraryViewModel = libraryViewModel;
        _settingsViewModel = settingsViewModel;
        _diagnosticsViewModel = diagnosticsViewModel;
    }

    private readonly LibraryViewModel _libraryViewModel;
    private readonly SettingsViewModel _settingsViewModel;
    private readonly DiagnosticsViewModel _diagnosticsViewModel;

    [RelayCommand]
    private void NavigateToLibrary()
    {
        CurrentView = _libraryViewModel;
        WindowTitle = "PS2 Home - Library";
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
}