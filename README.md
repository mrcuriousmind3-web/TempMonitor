# TempMonitor

A small Windows desktop temperature monitor built with C# WinForms and LibreHardwareMonitor.

## Features
- CPU, GPU and motherboard temperature sensors (when exposed by the hardware/firmware)
- 2-second refresh interval
- Minimal dark interface
- No telemetry, login, ads, or background service

## Build on Windows
1. Install the .NET 8 SDK.
2. Open this folder in a terminal.
3. Run `dotnet restore`.
4. Run `dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true`.
5. Find the app in `bin\\Release\\net8.0-windows\\win-x64\\publish\\TempMonitor.exe`.

The self-contained executable is larger than a framework-dependent build because it includes the .NET runtime. For a smaller output on a PC that already has the .NET 8 Desktop Runtime, publish with `--self-contained false`.

## Notes
- Some motherboard and CPU sensors require running the app as Administrator.
- Sensor availability depends on the PC, firmware, GPU vendor and driver.
- This app reports sensor readings; it does not control fan speeds or change thermal limits.
- Temperature labels are grouped by hardware device. Some systems expose multiple sensors with similar names.
