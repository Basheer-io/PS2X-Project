#include <iostream>
#include <string>
#include <vector>
#include <cstring>
#include <cstdint>

#if defined(_MSC_VER)
    #include <intrin.h>
    #include <windows.h>
#elif defined(__GNUC__) || defined(__clang__)
    #include <cpuid.h>
    #if defined(PLATFORM_WINDOWS)
        #include <windows.h>
    #else
        #include <dlfcn.h>
    #endif
#endif

// CPU Features struct
struct CpuFeatures {
    bool has_sse41 = false;
    bool has_avx2 = false;
    bool has_aes = false;
};

// Vulkan Probe struct
struct VulkanProbe {
    bool supported = false;
    std::string api_version = "";
    std::string device_name = "";
    std::string device_type = "Unknown";
    std::string error = "";
};

static CpuFeatures ProbeCpu() {
    CpuFeatures cpu;

#if defined(_MSC_VER)
    int info[4];
    // Function 1
    __cpuid(info, 1);
    cpu.has_sse41 = (info[2] & (1 << 19)) != 0;
    cpu.has_aes = (info[2] & (1 << 25)) != 0;

    // Function 7, subfunction 0
    __cpuidex(info, 7, 0);
    cpu.has_avx2 = (info[1] & (1 << 5)) != 0;

#elif defined(__x86_64__) || defined(__i386__)
    unsigned int eax = 0, ebx = 0, ecx = 0, edx = 0;

    // Function 1
    if (__get_cpuid(1, &eax, &ebx, &ecx, &edx)) {
        cpu.has_sse41 = (ecx & (1 << 19)) != 0;
        cpu.has_aes = (ecx & (1 << 25)) != 0;
    }

    // Function 7, subfunction 0
    if (__get_cpuid_count(7, 0, &eax, &ebx, &ecx, &edx)) {
        cpu.has_avx2 = (ebx & (1 << 5)) != 0;
    }
#endif

    return cpu;
}

// Minimal Vulkan definitions for runtime dynamic loading
typedef void* VkInstance;
typedef void* VkPhysicalDevice;
typedef int VkResult;

#define VK_STRUCTURE_TYPE_APPLICATION_INFO 0
#define VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO 1
#define VK_SUCCESS 0

struct VkApplicationInfo {
    int sType;
    const void* pNext;
    const char* pApplicationName;
    uint32_t applicationVersion;
    const char* pEngineName;
    uint32_t engineVersion;
    uint32_t apiVersion;
};

struct VkInstanceCreateInfo {
    int sType;
    const void* pNext;
    uint32_t flags;
    const VkApplicationInfo* pApplicationInfo;
    uint32_t enabledLayerCount;
    const char* const* ppEnabledLayerNames;
    uint32_t enabledExtensionCount;
    const char* const* ppEnabledExtensionNames;
};

typedef VkResult (*PFN_vkCreateInstance)(const VkInstanceCreateInfo*, const void*, VkInstance*);
typedef void (*PFN_vkDestroyInstance)(VkInstance, const void*);
typedef VkResult (*PFN_vkEnumeratePhysicalDevices)(VkInstance, uint32_t*, VkPhysicalDevice*);
typedef void (*PFN_vkGetPhysicalDeviceProperties)(VkPhysicalDevice, void*);

static std::string FormatVulkanVersion(uint32_t version) {
    uint32_t major = version >> 22;
    uint32_t minor = (version >> 12) & 0x3FF;
    uint32_t patch = version & 0xFFF;
    return std::to_string(major) + "." + std::to_string(minor) + "." + std::to_string(patch);
}

static std::string MapDeviceType(uint32_t type) {
    switch (type) {
        case 1: return "IntegratedGpu";
        case 2: return "DiscreteGpu";
        case 3: return "VirtualGpu";
        case 4: return "Cpu";
        default: return "Other";
    }
}

static VulkanProbe ProbeVulkan() {
    VulkanProbe vk;

#if defined(PLATFORM_WINDOWS) || defined(_WIN32)
    HMODULE lib = LoadLibraryA("vulkan-1.dll");
    if (!lib) {
        vk.error = "vulkan-1.dll not found on system";
        return vk;
    }
    auto get_sym = [lib](const char* name) { return (void*)GetProcAddress(lib, name); };
    auto close_lib = [lib]() { FreeLibrary(lib); };
#else
    void* lib = dlopen("libvulkan.so.1", RTLD_NOW | RTLD_LOCAL);
    if (!lib) {
        lib = dlopen("libvulkan.so", RTLD_NOW | RTLD_LOCAL);
    }
    if (!lib) {
        vk.error = "libvulkan.so.1 not found on system";
        return vk;
    }
    auto get_sym = [lib](const char* name) { return dlsym(lib, name); };
    auto close_lib = [lib]() { dlclose(lib); };
#endif

    auto vkCreateInstance = (PFN_vkCreateInstance)get_sym("vkCreateInstance");
    auto vkDestroyInstance = (PFN_vkDestroyInstance)get_sym("vkDestroyInstance");
    auto vkEnumeratePhysicalDevices = (PFN_vkEnumeratePhysicalDevices)get_sym("vkEnumeratePhysicalDevices");
    auto vkGetPhysicalDeviceProperties = (PFN_vkGetPhysicalDeviceProperties)get_sym("vkGetPhysicalDeviceProperties");

    if (!vkCreateInstance || !vkDestroyInstance || !vkEnumeratePhysicalDevices || !vkGetPhysicalDeviceProperties) {
        vk.error = "Failed to load core Vulkan function pointers";
        close_lib();
        return vk;
    }

    VkApplicationInfo appInfo{};
    appInfo.sType = VK_STRUCTURE_TYPE_APPLICATION_INFO;
    appInfo.pApplicationName = "PS2HomeDiag";
    appInfo.applicationVersion = 1;
    appInfo.apiVersion = (1 << 22) | (1 << 12); // Vulkan 1.1

    VkInstanceCreateInfo createInfo{};
    createInfo.sType = VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO;
    createInfo.pApplicationInfo = &appInfo;

    VkInstance instance = nullptr;
    VkResult res = vkCreateInstance(&createInfo, nullptr, &instance);
    if (res != VK_SUCCESS || !instance) {
        vk.error = "vkCreateInstance failed with code " + std::to_string(res);
        close_lib();
        return vk;
    }

    uint32_t deviceCount = 0;
    vkEnumeratePhysicalDevices(instance, &deviceCount, nullptr);

    if (deviceCount == 0) {
        vk.error = "No Vulkan physical devices detected";
        vkDestroyInstance(instance, nullptr);
        close_lib();
        return vk;
    }

    std::vector<VkPhysicalDevice> devices(deviceCount);
    vkEnumeratePhysicalDevices(instance, &deviceCount, devices.data());

    // 2048-byte buffer safely covers standard VkPhysicalDeviceProperties (~824 bytes)
    uint8_t propBuffer[2048];
    std::memset(propBuffer, 0, sizeof(propBuffer));

    vkGetPhysicalDeviceProperties(devices[0], propBuffer);

    uint32_t apiVersion = *reinterpret_cast<uint32_t*>(propBuffer);
    uint32_t deviceType = *reinterpret_cast<uint32_t*>(propBuffer + 16);
    const char* deviceName = reinterpret_cast<const char*>(propBuffer + 20);

    vk.supported = true;
    vk.api_version = FormatVulkanVersion(apiVersion);
    vk.device_name = std::string(deviceName);
    vk.device_type = MapDeviceType(deviceType);

    vkDestroyInstance(instance, nullptr);
    close_lib();
    return vk;
}

static void EscapeJsonString(std::ostream& os, const std::string& str) {
    for (char c : str) {
        if (c == '"') os << "\\\"";
        else if (c == '\\') os << "\\\\";
        else if (c == '\b') os << "\\b";
        else if (c == '\f') os << "\\f";
        else if (c == '\n') os << "\\n";
        else if (c == '\r') os << "\\r";
        else if (c == '\t') os << "\\t";
        else os << c;
    }
}

int main() {
    CpuFeatures cpu = ProbeCpu();
    VulkanProbe vk = ProbeVulkan();

    std::cout << "{\n";
    std::cout << "  \"schema_version\": 1,\n";

    // CPU Section
    std::cout << "  \"cpu\": {\n";
    std::cout << "    \"has_sse41\": " << (cpu.has_sse41 ? "true" : "false") << ",\n";
    std::cout << "    \"has_avx2\": " << (cpu.has_avx2 ? "true" : "false") << ",\n";
    std::cout << "    \"has_aes\": " << (cpu.has_aes ? "true" : "false") << "\n";
    std::cout << "  },\n";

    // Vulkan Section
    std::cout << "  \"vulkan\": {\n";
    std::cout << "    \"supported\": " << (vk.supported ? "true" : "false") << ",\n";
    std::cout << "    \"api_version\": \""; EscapeJsonString(std::cout, vk.api_version); std::cout << "\",\n";
    std::cout << "    \"device_name\": \""; EscapeJsonString(std::cout, vk.device_name); std::cout << "\",\n";
    std::cout << "    \"device_type\": \""; EscapeJsonString(std::cout, vk.device_type); std::cout << "\"\n";
    std::cout << "  },\n";

    // Errors Section
    std::cout << "  \"errors\": [";
    if (!vk.error.empty()) {
        std::cout << "\n    \"";
        EscapeJsonString(std::cout, vk.error);
        std::cout << "\"\n  ";
    }
    std::cout << "]\n";
    std::cout << "}\n";

    return 0;
}
