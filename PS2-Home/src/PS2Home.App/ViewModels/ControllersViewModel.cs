using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PS2Home.Core;

namespace PS2Home.App.ViewModels;

/// <summary>
/// ViewModel for the Controller Guidance page, assisting users with controller setup in PCSX2.
/// </summary>
public partial class ControllersViewModel : ViewModelBase
{
    private readonly ISettingsStore _settingsStore;
    private readonly IPcsx2Locator _pcsx2Locator;
    private readonly IProcessRunner _processRunner;

    public const string ControllersDocUrl = "https://pcsx2.net/docs/setup/controllers/";

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _isLaunchingPcsx2 = false;

    [ObservableProperty]
    private string _docUrl = ControllersDocUrl;

    public ControllersViewModel() : this(
        new PS2Home.Infrastructure.JsonSettingsStore(),
        new PS2Home.Infrastructure.Pcsx2Locator(),
        new PS2Home.Infrastructure.DefaultProcessRunner())
    {
    }

    public ControllersViewModel(
        ISettingsStore settingsStore,
        IPcsx2Locator pcsx2Locator,
        IProcessRunner processRunner)
    {
        _settingsStore = settingsStore;
        _pcsx2Locator = pcsx2Locator;
        _processRunner = processRunner;
    }

    [RelayCommand]
    public async Task LaunchPcsx2Async()
    {
        if (IsLaunchingPcsx2) return;

        IsLaunchingPcsx2 = true;
        StatusMessage = "Locating PCSX2...";

        try
        {
            var settings = await _settingsStore.LoadSettingsAsync();
            var install = settings.Pcsx2;

            if (install == null || string.IsNullOrWhiteSpace(install.Command))
            {
                var detected = await _pcsx2Locator.DetectInstallationsAsync();
                if (detected.Count > 0)
                {
                    install = detected[0];
                }
            }

            if (install == null || string.IsNullOrWhiteSpace(install.Command))
            {
                StatusMessage = "PCSX2 not found. Please install PCSX2 or configure its executable path in Settings.";
                return;
            }

            StatusMessage = $"Launching {install.DisplayName}...";

            // Modern PCSX2 with no arguments opens the PCSX2 main GUI window
            var args = install.Kind == Pcsx2InstallKind.Flatpak
                ? new[] { "run", "net.pcsx2.PCSX2" }
                : Array.Empty<string>();

            var command = install.Kind == Pcsx2InstallKind.Flatpak ? "flatpak" : install.Command;

            _ = Task.Run(() =>
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = command,
                        UseShellExecute = false
                    };
                    foreach (var a in args)
                    {
                        psi.ArgumentList.Add(a);
                    }
                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Failed to start PCSX2: {ex.Message}";
                }
            });

            StatusMessage = "PCSX2 launched! Configure your controller in PCSX2 under Settings → Controllers.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error launching PCSX2: {ex.Message}";
        }
        finally
        {
            IsLaunchingPcsx2 = false;
        }
    }

    [RelayCommand]
    public void OpenDocumentation()
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo { FileName = DocUrl, UseShellExecute = true });
            }
            else if (OperatingSystem.IsLinux())
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "xdg-open",
                    UseShellExecute = false
                };
                psi.ArgumentList.Add(DocUrl);
                Process.Start(psi);
            }
            StatusMessage = "Opened official PCSX2 controller documentation in browser.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Documentation URL: {DocUrl} ({ex.Message})";
        }
    }
}
