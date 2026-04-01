# Godot CLI Setup

## Prerequisites
- `.NET 10 SDK`
- `godot-mcp` available on `PATH`
- Godot 4.x executable (optional for text-only operations, required for CLI-backed operations)

## Install MCP Server Tool

```bash
dotnet tool install --global GodotMcp.Server
godot-mcp --version
```

If the executable is not on `PATH`, configure plugin option `ExecutablePath` or set:

```bash
GODOT_MCP_PATH=/absolute/path/to/godot-mcp
```

## Configure Godot Executable

When GD tools need engine-backed operations, set:

```bash
GODOT_PATH=/absolute/path/to/godot
```

Or set `GodotMcp:GodotExecutablePath` in `appsettings.json`.

The plugin forwards `GodotExecutablePath` as `GODOT_PATH` when starting `godot-mcp`.
