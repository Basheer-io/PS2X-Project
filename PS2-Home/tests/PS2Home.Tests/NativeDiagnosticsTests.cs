using PS2Home.Core;
using PS2Home.Infrastructure;
using Xunit;

namespace PS2Home.Tests;

public class NativeDiagnosticsTests
{
    private const string SampleJson = """
    {
      "schema_version": 1,
      "cpu": {
        "has_sse41": true,
        "has_avx2": true,
        "has_aes": true
      },
      "vulkan": {
        "supported": true,
        "api_version": "1.3.268",
        "device_name": "NVIDIA GeForce RTX 4070",
        "device_type": "DiscreteGpu"
      },
      "errors": []
    }
    """;

    [Fact]
    public async Task QueryDiagnosticsAsync_ValidJson_ParsesCpuAndGpuInfo()
    {
        var runner = new FakeProcessRunner
        {
            Handler = (_, _) => new ProcessExecutionResult(0, SampleJson, string.Empty, false)
        };

        var dummyPath = Path.GetTempFileName();
        try
        {
            var diag = new NativeDiagnostics(runner, dummyPath);
            var result = await diag.QueryDiagnosticsAsync();

            Assert.True(result.IsAvailable);
            Assert.NotNull(result.Cpu);
            Assert.True(result.Cpu.HasSse41);
            Assert.True(result.Cpu.HasAvx2);
            Assert.True(result.Cpu.HasAes);

            Assert.NotNull(result.Gpu);
            Assert.True(result.Gpu.Supported);
            Assert.Equal("1.3.268", result.Gpu.ApiVersion);
            Assert.Equal("NVIDIA GeForce RTX 4070", result.Gpu.DeviceName);
            Assert.Equal("DiscreteGpu", result.Gpu.DeviceType);
            Assert.Empty(result.Errors);
        }
        finally
        {
            try { File.Delete(dummyPath); } catch { }
        }
    }

    [Fact]
    public async Task QueryDiagnosticsAsync_WhenBinaryMissing_ReturnsUnavailableGracefully()
    {
        var runner = new FakeProcessRunner();
        var diag = new NativeDiagnostics(runner, "/non/existent/path/ps2home-diag");

        var result = await diag.QueryDiagnosticsAsync();

        Assert.False(result.IsAvailable);
        Assert.NotEmpty(result.Errors);
        Assert.Contains("not found", result.Errors[0], StringComparison.OrdinalIgnoreCase);
        Assert.Empty(runner.ExecutedCalls);
    }

    [Fact]
    public async Task QueryDiagnosticsAsync_WhenProcessTimesOut_ReturnsUnavailable()
    {
        var runner = new FakeProcessRunner
        {
            Handler = (_, _) => new ProcessExecutionResult(-1, string.Empty, "Timed out", true)
        };

        var dummyPath = Path.GetTempFileName();
        try
        {
            var diag = new NativeDiagnostics(runner, dummyPath);
            var result = await diag.QueryDiagnosticsAsync();

            Assert.False(result.IsAvailable);
            Assert.NotEmpty(result.Errors);
            Assert.Contains("timed out", result.Errors[0], StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            try { File.Delete(dummyPath); } catch { }
        }
    }

    [Fact]
    public async Task QueryDiagnosticsAsync_WhenJsonIsCorrupt_ReturnsUnavailableWithoutCrashing()
    {
        var runner = new FakeProcessRunner
        {
            Handler = (_, _) => new ProcessExecutionResult(0, "--- not valid json ---", string.Empty, false)
        };

        var dummyPath = Path.GetTempFileName();
        try
        {
            var diag = new NativeDiagnostics(runner, dummyPath);
            var result = await diag.QueryDiagnosticsAsync();

            Assert.False(result.IsAvailable);
            Assert.NotEmpty(result.Errors);
            Assert.Contains("invalid json", result.Errors[0], StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            try { File.Delete(dummyPath); } catch { }
        }
    }
}
