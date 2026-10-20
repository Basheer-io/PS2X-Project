Read AGENTS.md first and follow it strictly.

**Phase 5 (optional for v1.0): C++ native diagnostics helper.** Only proceed if there is at least one concrete diagnostic that needs native access and cannot be done well in C# (for example, GPU/driver details or a Vulkan capability probe). If nothing justifies it, say so, document the decision in docs/architecture.md, and stop.

Tasks:
1. Write down the chosen diagnostics and why native code is needed, in docs/native-helper.md.
2. Create `native/PS2Home.Native/` with CMake. Build a small standalone executable (preferred over in-process P/Invoke because crashes cannot take down the app) that prints a single JSON document to stdout with a version field, results and structured errors. Exit codes documented.
3. Implement only the justified checks; handle unsupported platforms by returning a structured "unsupported" result.
4. C#: an `INativeDiagnostics` interface in Core; an Infrastructure implementation that locates and runs the helper with a timeout, parses JSON, and degrades gracefully when the helper is missing, crashes, times out or returns bad JSON. Show results in Diagnostics.
5. Tests: C# parsing and failure handling with fake outputs; a native test or smoke test run in CI on Linux and Windows.
6. Add to CI. Document building the helper on Fedora and Windows. Keep licensing clean (no copied code without compatible license).

Acceptance: the app calls the helper and interprets results; when the helper is absent or fails the app continues; native module builds and is tested per platform.

Report in the format required by AGENTS.md.
