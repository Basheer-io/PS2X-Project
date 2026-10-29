using System.Text.Json.Serialization;

namespace PS2Home.Core;

/// <summary>
/// CPU instruction set extensions probed natively via CPUID.
/// </summary>
public record NativeCpuInfo
{
    [JsonPropertyName("has_sse41")]
    public bool HasSse41 { get; init; }

    [JsonPropertyName("has_avx2")]
    public bool HasAvx2 { get; init; }

    [JsonPropertyName("has_aes")]
    public bool HasAes { get; init; }
}
