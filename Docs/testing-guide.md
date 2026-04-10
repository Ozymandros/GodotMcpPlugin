# Testing Guide

## Run All Tests

```bash
dotnet test GodotMcp.sln
```

## Run with Coverage

```bash
dotnet test GodotMcp.sln --collect:"XPlat Code Coverage" --settings coverlet.runsettings
```

Coverage thresholds are enforced from `coverlet.runsettings`:
- line: 60%
- branch: 60%
- method: 60%

To run the same coverage gate used in CI/local validation:

```bash
dotnet test GodotMcp.sln -c Release --collect:"XPlat Code Coverage" --settings coverlet.runsettings
```

Coverage artifacts are produced under `TestResults/`.

## Test Categories
- `InfrastructureTests`: stdio transport, retries, process lifecycle, serialization, parameter conversion.
- `PluginTests`: function mapping, SK registration (expanded + router), validation and security checks.
- `CoreTests`: model invariants and sanitizer behavior.
- FsCheck property tests for serialization/roundtrip guarantees.

## Integration Notes
- Tests are designed to run with mocks by default.
- Real server integration can be added by setting up `godot-mcp` and environment variables, then adding opt-in test filters in CI.
