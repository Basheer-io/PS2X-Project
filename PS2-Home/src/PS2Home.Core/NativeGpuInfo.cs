using System.Text.Json.Serialization;

namespace PS2Home.Core;

/// <summary>
/// GPU and Vulkan driver capability information probed via dynamic runtime loading.
/// </summary>
public record NativeGpuInfo
{
    [JsonPropertyName("supported")]
    public bool Supported { get; init; }

    [JsonPropertyName("api_version")]
    public string? ApiVersion { get; init; }

    [JsonPropertyName("device_name")]
    public string? DeviceName { get; init; }

    [JsonPropertyName("device_type")]
    public string? DeviceType { get; init; }
}
