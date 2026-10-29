# Third-Party Software Notices and Information

This project incorporates third-party open-source software packages and components.
Below is a listing of the components, their licenses, and copyright notices.

---

## 1. Summary of Dependencies

| Package / Component | License | Project URL / Source | Purpose |
|---|---|---|---|
| **Avalonia** | MIT | https://github.com/AvaloniaUI/Avalonia | Cross-platform XAML GUI framework |
| **Avalonia.Desktop** | MIT | https://github.com/AvaloniaUI/Avalonia | Desktop platform integration |
| **Avalonia.Themes.Fluent** | MIT | https://github.com/AvaloniaUI/Avalonia | Fluent design system styles and brushes |
| **Avalonia.Fonts.Inter** | MIT / SIL OFL 1.1 | https://github.com/rsms/inter | Typography (Inter Font Family) |
| **CommunityToolkit.Mvvm** | MIT | https://github.com/CommunityToolkit/dotnet | MVVM architecture, observable properties, commands |
| **Microsoft.Extensions.DependencyInjection** | MIT | https://github.com/dotnet/runtime | IoC and dependency injection container |
| **Microsoft.Extensions.DependencyInjection.Abstractions** | MIT | https://github.com/dotnet/runtime | DI abstractions |
| **xUnit.net (v3)** | Apache-2.0 | https://github.com/xunit/xunit | Automated testing framework (build/test only) |
| **Microsoft.NET.Test.Sdk** | MIT | https://github.com/microsoft/vstest | Test runner SDK (build/test only) |
| **Vulkan Headers** | Apache-2.0 | https://github.com/KhronosGroup/Vulkan-Headers | Vulkan dynamic loader header definitions (native probe) |

---

## 2. Non-Bundled Software & Anti-Piracy Statement

- **PCSX2**: PCSX2 is **NOT** bundled, distributed, or linked with PS2 Home. PS2 Home is an independent frontend and launcher that invokes a user-installed, separate copy of PCSX2 via standard command-line interfaces.
- **BIOS Files & Game ROMs**: PS2 Home does **NOT** include, download, or link to PlayStation 2 BIOS ROM images or game disc images. Users must dump their own BIOS from physical hardware they own and use their own legally obtained game images.

---

## 3. Licenses

### MIT License
Used by: Avalonia, CommunityToolkit.Mvvm, Microsoft.Extensions.DependencyInjection, Microsoft.NET.Test.Sdk

```
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

### SIL Open Font License (OFL) 1.1
Used by: Inter Font (embedded via Avalonia.Fonts.Inter)

```
Copyright (c) 2016-2020 The Inter Project Authors (https://github.com/rsms/inter)

Permission is hereby granted, free of charge, to any person obtaining a copy
of the Font Software, to use, study, copy, merge, embed, modify, redistribute,
and sell modified and unmodified copies of the Font Software, subject to the
conditions stated in the SIL Open Font License 1.1.
```

### Apache License 2.0
Used by: xUnit.net, Khronos Vulkan Headers

```
Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
```
