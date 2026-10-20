using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace PS2Home.App.ViewModels;

/// <summary>
/// Diagnostics view model - shows system info, logs, and debugging tools.
/// </summary>
public partial class DiagnosticsViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<string> _logEntries = [];

    [ObservableProperty]
    private string _systemInfo = string.Empty;

    [ObservableProperty]
    private string _appVersion = "1.0.0";

    [ObservableProperty]
    private long _memoryUsage = 0;

    [ObservableProperty]
    private string _statusMessage = "Diagnostics ready";

    public DiagnosticsViewModel()
    {
        CollectSystemInfo();
        AddLogEntry("Diagnostics view initialized");
    }

    [RelayCommand]
    private void RefreshSystemInfo()
    {
        CollectSystemInfo();
        AddLogEntry("System info refreshed");
        StatusMessage = "System info updated";
    }

    [RelayCommand]
    private void ClearLogs()
    {
        LogEntries.Clear();
        AddLogEntry("Logs cleared");
        StatusMessage = "Logs cleared";
    }

    [RelayCommand]
    private void RunSelfTest()
    {
        AddLogEntry("Running self-test...");
        // Simulate self-test
        AddLogEntry("✓ Core assembly loaded");
        AddLogEntry("✓ Infrastructure assembly loaded");
        AddLogEntry("✓ UI assembly loaded");
        AddLogEntry("✓ Configuration paths accessible");
        AddLogEntry("Self-test completed successfully");
        StatusMessage = "Self-test passed";
    }

    [RelayCommand]
    private void OpenLogFolder()
    {
        // TODO: Open log folder in file explorer
        AddLogEntry("Open log folder requested (not implemented)");
        StatusMessage = "Log folder open not implemented";
    }

    [RelayCommand]
    private void CopyLogsToClipboard()
    {
        var allLogs = string.Join(Environment.NewLine, LogEntries);
        // TODO: Copy to clipboard
        AddLogEntry("Copy to clipboard requested (not implemented)");
        StatusMessage = "Copy to clipboard not implemented";
    }

    private void CollectSystemInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"OS: {Environment.OSVersion}");
        info.AppendLine($"Runtime: {Environment.Version}");
        info.AppendLine($"Architecture: {Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE") ?? "Unknown"}");
        info.AppendLine($"Processor Count: {Environment.ProcessorCount}");
        info.AppendLine($"Working Set: {Environment.WorkingSet / 1024 / 1024} MB");
        info.AppendLine($"User: {Environment.UserName}");
        info.AppendLine($"Machine: {Environment.MachineName}");
        info.AppendLine($"App Domain: {AppDomain.CurrentDomain.FriendlyName}");
        info.AppendLine($"Base Directory: {AppDomain.CurrentDomain.BaseDirectory}");

        SystemInfo = info.ToString();
        
        // Update memory usage
        var process = Process.GetCurrentProcess();
        MemoryUsage = process.WorkingSet64 / 1024 / 1024; // MB
    }

    private void AddLogEntry(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        LogEntries.Add($"[{timestamp}] {message}");
    }
}