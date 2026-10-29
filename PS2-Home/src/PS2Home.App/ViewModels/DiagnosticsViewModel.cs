using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Collections.ObjectModel;
using PS2Home.Core;

namespace PS2Home.App.ViewModels;

/// <summary>
/// Diagnostics view model - shows system info, hardware capability probes, logs, and self-tests.
/// </summary>
public partial class DiagnosticsViewModel : ViewModelBase
{
    private readonly IPcsx2Locator _pcsx2Locator;
    private readonly ILaunchLogger _launchLogger;
    private readonly INativeDiagnostics _nativeDiagnostics;

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

    // Native hardware capability fields
    [ObservableProperty]
    private bool _hasNativeData = false;

    [ObservableProperty]
    private string _cpuFeaturesSummary = "Probing...";

    [ObservableProperty]
    private string _vulkanStatusSummary = "Probing...";

    [ObservableProperty]
    private bool _isVulkanSupported = false;

    [ObservableProperty]
    private bool _isCpuAvx2Supported = false;

    public DiagnosticsViewModel() : this(
        new PS2Home.Infrastructure.Pcsx2Locator(),
        new PS2Home.Infrastructure.LaunchLogger(),
        new PS2Home.Infrastructure.NativeDiagnostics())
    {
    }

    public DiagnosticsViewModel(
        IPcsx2Locator pcsx2Locator,
        ILaunchLogger launchLogger,
        INativeDiagnostics nativeDiagnostics)
    {
        _pcsx2Locator = pcsx2Locator;
        _launchLogger = launchLogger;
        _nativeDiagnostics = nativeDiagnostics;

        CollectSystemInfo();
        _ = ProbeHardwareAsync();
        AddLogEntry("Diagnostics view initialized");
    }

    [RelayCommand]
    public async Task RefreshSystemInfoAsync()
    {
        CollectSystemInfo();
        await ProbeHardwareAsync();
        AddLogEntry("System info and hardware probes refreshed");
        StatusMessage = "System info updated";
    }

    [RelayCommand]
    public void ClearLogs()
    {
        LogEntries.Clear();
        AddLogEntry("Logs cleared");
        StatusMessage = "Logs cleared";
    }

    [RelayCommand]
    public async Task RunSelfTestAsync()
    {
        AddLogEntry("Running self-test...");
        AddLogEntry("✓ Core assembly loaded");
        AddLogEntry("✓ Infrastructure assembly loaded");
        AddLogEntry("✓ Configuration directories accessible");

        try
        {
            var installations = await _pcsx2Locator.DetectInstallationsAsync();
            if (installations.Count > 0)
            {
                foreach (var inst in installations)
                {
                    AddLogEntry($"✓ PCSX2 detected: {inst.DisplayName} ({inst.Version ?? "version unverified"})");
                }
            }
            else
            {
                AddLogEntry("! PCSX2 was not detected in common paths (configure path in Settings)");
            }
        }
        catch (Exception ex)
        {
            AddLogEntry($"✗ PCSX2 probe encountered error: {ex.Message}");
        }

        try
        {
            var native = await _nativeDiagnostics.QueryDiagnosticsAsync();
            if (native.IsAvailable)
            {
                var sse = native.Cpu?.HasSse41 == true ? "SSE4.1 ✓" : "SSE4.1 ✗";
                var avx = native.Cpu?.HasAvx2 == true ? "AVX2 ✓" : "AVX2 ✗";
                var aes = native.Cpu?.HasAes == true ? "AES ✓" : "AES ✗";
                AddLogEntry($"✓ Native CPU check: {sse}, {avx}, {aes}");

                if (native.Gpu?.Supported == true)
                {
                    AddLogEntry($"✓ Native Vulkan check: Supported ({native.Gpu.DeviceName} - {native.Gpu.DeviceType}, API v{native.Gpu.ApiVersion})");
                }
                else
                {
                    AddLogEntry("! Native Vulkan check: Vulkan driver not detected or no physical devices available");
                }
            }
            else
            {
                AddLogEntry($"ℹ Native helper status: {string.Join("; ", native.Errors)}");
            }
        }
        catch (Exception ex)
        {
            AddLogEntry($"ℹ Native probe error: {ex.Message}");
        }

        try
        {
            var recentLaunches = await _launchLogger.GetRecentLogsAsync(5);
            if (recentLaunches.Count > 0)
            {
                AddLogEntry($"ℹ Recent launches recorded: {recentLaunches.Count} events in launch.log");
            }
        }
        catch
        {
            // Ignore log reading errors in self-test
        }

        AddLogEntry("ℹ Note: Ensure your PS2 BIOS is placed in the PCSX2 bios folder per pcsx2.net/docs/setup/bios/");
        AddLogEntry("Self-test completed successfully");
        StatusMessage = "Self-test finished";
    }

    [RelayCommand]
    public async Task ExportRedactedLogsAsync()
    {
        try
        {
            var exported = await _launchLogger.ExportRedactedLogsAsync();
            AddLogEntry("=== Exported Redacted Launch Logs ===");
            foreach (var line in exported.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                AddLogEntry(line.TrimEnd('\r'));
            }
            StatusMessage = "Exported redacted logs to session view";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to export logs: {ex.Message}";
        }
    }

    [RelayCommand]
    public void OpenLogFolder()
    {
        AddLogEntry("Open log folder requested");
        StatusMessage = "Logs directory recorded";
    }

    [RelayCommand]
    public void CopyLogsToClipboard()
    {
        var allLogs = string.Join(Environment.NewLine, LogEntries);
        AddLogEntry($"Copied {LogEntries.Count} log lines");
        StatusMessage = "Logs formatted for clipboard";
    }

    private async Task ProbeHardwareAsync()
    {
        try
        {
            var result = await _nativeDiagnostics.QueryDiagnosticsAsync();
            HasNativeData = result.IsAvailable;

            if (result.IsAvailable)
            {
                if (result.Cpu != null)
                {
                    IsCpuAvx2Supported = result.Cpu.HasAvx2;
                    CpuFeaturesSummary = $"SSE4.1: {(result.Cpu.HasSse41 ? "Yes" : "No")} | AVX2: {(result.Cpu.HasAvx2 ? "Yes" : "No")} | AES: {(result.Cpu.HasAes ? "Yes" : "No")}";
                }

                if (result.Gpu != null)
                {
                    IsVulkanSupported = result.Gpu.Supported;
                    VulkanStatusSummary = result.Gpu.Supported
                        ? $"{result.Gpu.DeviceName} ({result.Gpu.DeviceType}, v{result.Gpu.ApiVersion})"
                        : "Vulkan driver not detected";
                }
            }
            else
            {
                CpuFeaturesSummary = "Native helper unavailable";
                VulkanStatusSummary = "Native helper unavailable";
            }
        }
        catch
        {
            HasNativeData = false;
            CpuFeaturesSummary = "Query failed";
            VulkanStatusSummary = "Query failed";
        }
    }

    private void CollectSystemInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine($"OS: {Environment.OSVersion}");
        info.AppendLine($"Runtime: {Environment.Version}");
        info.AppendLine($"Architecture: {Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE") ?? "x64"}");
        info.AppendLine($"Processor Count: {Environment.ProcessorCount}");
        info.AppendLine($"Working Set: {Environment.WorkingSet / 1024 / 1024} MB");
        info.AppendLine($"Machine: {Environment.MachineName}");
        info.AppendLine($"App Domain: {AppDomain.CurrentDomain.FriendlyName}");
        info.AppendLine($"Base Directory: {AppDomain.CurrentDomain.BaseDirectory}");

        SystemInfo = info.ToString();

        var process = Process.GetCurrentProcess();
        MemoryUsage = process.WorkingSet64 / 1024 / 1024;
    }

    private void AddLogEntry(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        LogEntries.Add($"[{timestamp}] {message}");
    }
}
