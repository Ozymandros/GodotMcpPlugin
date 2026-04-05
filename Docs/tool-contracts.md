# Tool Contracts

This plugin is discovery-first: it connects with the official .NET MCP client (`ModelContextProtocol` NuGet, aligned with **GodotMCP.Server 1.2.x**), performs MCP `initialize`, then uses **`tools/list`** and **`tools/call`** for discovery and invocation. Tool names and parameters match the server’s advertised tool surface.

The current **GodotMCP.Server** tool names (examples) include:

## Core
- `health_check()`
- `get_server_info()`
- `get_server_capabilities()`
- `get_godot_version()`

## Project
- `create_godot_project(projectName)`
- `get_project_info()`
- `configure_autoload(key, value, enabled)`
- `add_plugin(pluginName)`

## Scene and Node
- `create_scene(scenePath, rootNodeName, rootNodeType)`
- `add_node(scenePath, parentPath, nodeName, nodeType)`
- `set_node_property(scenePath, nodeName, propertyKey, propertyValue)`
- `remove_node(scenePath, nodeName)`
- `instantiate_packed_scene(targetScenePath, parentPath, packedScenePath, instanceName)`
- `save_branch_as_scene(sourceScenePath, nodeName, destinationScenePath)`

## Scripts
- `create_script(path, language, baseType, className)`
- `attach_script(scenePath, nodeName, scriptPath)`
- `validate_script(scriptPath, isCSharp)`

## Resources and Assets
- `create_resource(path, type, properties)`
- `generate_import_file(assetPath, importer, type, parameters?)`
- `reimport_asset(assetPath)`
- `create_texture(texturePath)`
- `create_audio(audioPath)`

## Editor and Export
- `run_editor_command(arguments)`
- `manage_export_presets(presetName, platform)`

## Integrations
- `discover_integrations()`
- `install_integration(integrationName, source, profile)`
- `enable_plugin(pluginName, enabled)`
- `verify_integration_health(integrationName)`
- `list_integration_compatibility()`
- `update_resource_uids(paths)`

## Response Envelope

Most tool methods return a `ToolResult` object:

```json
{
  "success": true,
  "message": "Operation completed",
  "data": {
    "key": "value"
  },
  "suggestedRemediation": null
}
```

If MCP returns a JSON-RPC error object, the plugin maps it to `McpServerException`.
