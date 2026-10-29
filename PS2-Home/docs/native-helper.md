# Native Diagnostics Helper (`ps2home-diag`)

## Justification
PS2 Home launches PCSX2, which relies heavily on hardware capabilities for real-time PS2 emulation:
1. **CPU Vector/JIT Extensions**: PCSX2's Emotion Engine (EE) and Vector Unit (VU) JIT recompilers require specific x86/x64 instruction set extensions (SSE4.1, AVX2, AES) to run games at full 60 FPS speed.
2. **GPU & Vulkan Driver Support**: PCSX2 performs best on modern systems using the Vulkan graphics backend. Checking if the Vulkan loader (`libvulkan.so.1` or `vulkan-1.dll`) is functional and querying the physical GPU device name and type (Discrete vs. Integrated) helps diagnose emulation performance bottlenecks before game launch.

## Architecture: Standalone CLI vs. In-Process P/Invoke
We deliberately chose a **standalone out-of-process CLI executable** (`ps2home-diag`) over in-process P/Invoke (`[DllImport]`):
- **Crash Isolation**: Incompatible graphics drivers, missing loader libraries, or buggy OpenGL/Vulkan ICDs can generate hardware-level segmentation faults or access violations. In an in-process P/Invoke call, this terminates the entire .NET runtime and desktop UI. With an out-of-process tool, only the child probe process crashes, which PS2 Home safely intercepts, reports, and handles gracefully.
- **Dynamic Runtime Linking**: The native helper uses `dlopen`/`dlsym` on Linux and `LoadLibrary`/`GetProcAddress` on Windows to check for Vulkan runtime support. It does not hard-link against Vulkan SDK headers or libraries.
- **Single Structured JSON Contract**: The tool outputs a single JSON object to `stdout` and exits with code `0`.

## Output Schema
```json
{
  "schema_version": 1,
  "cpu": {
    "has_sse41": true,
    "has_avx2": true,
    "has_aes": true
  },
  "vulkan": {
    "supported": true,
    "api_version": "1.3.0",
    "device_name": "NVIDIA GeForce RTX",
    "device_type": "DiscreteGpu"
  },
  "errors": []
}
```

## Exit Codes
- `0`: Success (valid JSON document printed to stdout).
- `1`: Unexpected fatal initialization error.
