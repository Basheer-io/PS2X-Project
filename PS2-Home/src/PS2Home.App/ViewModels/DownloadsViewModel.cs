using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PS2Home.App.ViewModels;

public partial class DownloadsViewModel : ViewModelBase
{
    public const string Pcsx2DownloadsUrl = "https://pcsx2.net/downloads/";
    public const string Pcsx2DocsUrl = "https://pcsx2.net/docs/";

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [RelayCommand]
    public void OpenPcsx2Downloads() => OpenUrl(Pcsx2DownloadsUrl);

    [RelayCommand]
    public void OpenPcsx2Docs() => OpenUrl(Pcsx2DocsUrl);

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
                var processStartInfo = new ProcessStartInfo("xdg-open")
                {
                    UseShellExecute = false
                };
                processStartInfo.ArgumentList.Add(url);
                Process.Start(processStartInfo);
            }

            StatusMessage = $"Opened {url}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not open browser: {ex.Message}";
        }
    }
}
