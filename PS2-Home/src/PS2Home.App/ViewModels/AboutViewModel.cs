using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PS2Home.Core;

namespace PS2Home.App.ViewModels;

/// <summary>
/// ViewModel for the About view, providing versioning, licensing, disclaimers, and project links.
/// </summary>
public partial class AboutViewModel : ViewModelBase
{
    public const string RepositoryUrl = "https://github.com/Basheer-io/PS2X-Project";
    public const string Pcsx2WebsiteUrl = "https://pcsx2.net";
    public const string BiosDocUrl = "https://pcsx2.net/docs/setup/bios/";

    [ObservableProperty]
    private string _appName = AppInfo.Name;

    [ObservableProperty]
    private string _version = $"v{AppInfo.Version}";

    [ObservableProperty]
    private string _tagline = AppInfo.Tagline;

    [ObservableProperty]
    private string _license = "MIT License";

    [ObservableProperty]
    private string _copyright = "Copyright © 2026 PS2 Home Contributors";

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public const string TrademarkDisclaimer =
        "PS2 Home is an independent open-source project and is not affiliated with, authorized, maintained, " +
        "sponsored, or endorsed by Sony Interactive Entertainment Inc. or the PCSX2 development team. " +
        "'PlayStation', 'PS2', 'DualShock', and 'DualSense' are registered trademarks or trademarks of " +
        "Sony Interactive Entertainment Inc.";

    public const string EthicalStatement =
        "PS2 Home does not distribute, download, or link to copyrighted game images (ISOs), BIOS dumps, " +
        "or PCSX2 binaries. Users must legally obtain their own games and dump their BIOS from their own physical console.";

    [RelayCommand]
    public void OpenGitHub() => OpenUrl(RepositoryUrl);

    [RelayCommand]
    public void OpenPcsx2() => OpenUrl(Pcsx2WebsiteUrl);

    [RelayCommand]
    public void OpenBiosGuide() => OpenUrl(BiosDocUrl);

    private void OpenUrl(string url)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            else if (OperatingSystem.IsLinux())
            {
                var psi = new ProcessStartInfo { FileName = "xdg-open", UseShellExecute = false };
                psi.ArgumentList.Add(url);
                Process.Start(psi);
            }
            StatusMessage = $"Opened {url}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not open browser: {ex.Message}";
        }
    }
}
