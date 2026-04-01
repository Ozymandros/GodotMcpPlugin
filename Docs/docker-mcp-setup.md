# Docker MCP Setup

Use this when your SK host runs in a container and needs to communicate with `godot-mcp`.

## Option 1: Bundle `godot-mcp` in Host Container
- Install the tool in image build.
- Configure:
  - `GodotMcp:ExecutablePath=godot-mcp`
  - `GODOT_PATH` if needed.

This repository includes a sample `Dockerfile` that builds and runs `samples/GodotMcp.Sample`.

## Option 2: Sidecar Pattern
- Run `godot-mcp` in a sibling container and expose a transport bridge if your host does not support stdio process launch directly.
- For this plugin, stdio process launching is expected; Option 1 is usually simpler.

## Notes
- Keep stdout clean for JSON-RPC only.
- Send diagnostics to stderr.
- For CI smoke tests, use mocked server process tests from `GodotMcp.Tests`.
