# Tool Contracts

This plugin is discovery-first: it connects with the official .NET MCP client (`ModelContextProtocol` NuGet, aligned with **GodotMCP.Server 1.2+**), performs MCP `initialize`, then uses **`tools/list`** and **`tools/call`** for discovery and invocation. Tool names and parameters match the server’s advertised tool surface.

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

Notes:
- The plugin exposes typed project wrappers and SK project skill methods for create/info/autoload/plugin workflows.

## Scene and Node
- `create_scene(scenePath, rootNodeName, rootNodeType)`
- `add_node(scenePath, parentPath, nodeName, nodeType)`
- `set_node_property(scenePath, nodeName, propertyKey, propertyValue)`
- `remove_node(scenePath, nodeName)`
- `instantiate_packed_scene(targetScenePath, parentPath, packedScenePath, instanceName)`
- `save_branch_as_scene(sourceScenePath, nodeName, destinationScenePath)`

## Camera (newer server versions)
Camera tool names are discovered dynamically, and newer releases can expose methods for:
- querying camera settings for nodes/scenes
- updating camera projection properties (e.g., perspective/orthogonal)
- changing clipping configuration (near/far)
- applying batched camera setting updates

## Scripts
- `create_script(path, language, baseType, className)`
- `attach_script(scenePath, nodeName, scriptPath)`
- `validate_script(scriptPath, isCSharp)`

Notes:
- The plugin exposes typed script wrappers and SK script skill methods for create/attach/validate workflows.

## Resources and Assets
- `create_resource(path, type, properties)`
- `generate_import_file(assetPath, importer, type, parameters?)`
- `reimport_asset(assetPath)`
- `create_texture(texturePath)`
- `create_audio(audioPath)`

Notes:
- The plugin exposes typed import wrappers and SK import skill methods for import file generation and asset reimport workflows.
- The plugin resource typed wrappers are compatibility-aware and can use either modern (`create_resource`, `resource.update_properties`) or legacy (`resource.create`, `resource.update`) command names based on server support.

## UI Module (`ui.*`)
- `ui.list_controls(scenePath)`
- `ui.add_control(scenePath, parentNodePath, controlType, nodeName, properties?)`
- `ui.set_control_properties(scenePath, controlNodePath, properties)`
- `ui.set_layout_preset(scenePath, controlNodePath, preset)`
- `ui.list_themes(scenePath)`
- `ui.apply_theme(scenePath, controlPath, themeName)`

Notes:
- The plugin keeps fallback compatibility for legacy aliases (`ui.create_control`, `ui.update_control`, `ui.apply_layout_preset`).
- Theme commands are plugin-typed extensions and require server support when available.

## Lighting Module (`light.*`)
- `light.list(scenePath)`
- `light.create(scenePath, parentPath, lightName, lightType)`
- `light.update(scenePath, lightPath, properties)`
- `light.tune(scenePath, lightPath, properties)`
- `light.validate(scenePath)`

Notes:
- `light.tune` is intended for iterative light property adjustment workflows.

## Physics Module (`physics.*`)
- `physics.list_bodies(projectRootPath)`
- `physics.create_body(scenePath, parentNodePath, bodyType, nodeName, addCollisionShape?)`
- `physics.update_body(scenePath, nodePath, properties)`
- `physics.list_shapes(scenePath)`
- `physics.create_shape(scenePath, bodyPath, shapeName, shapeType, properties)`
- `physics.update_shape(scenePath, shapePath, properties)`
- `physics.set_layers(scenePath, bodyPath, collisionLayer, collisionMask)`
- `physics.run_checks(scenePath, bodyPath?)`
- `physics.validate(projectRootPath)`

Notes:
- The plugin provides typed results for layer updates and physics checks to support collider/layer validation loops.
- Body commands are aligned with the current GD_MCP-Server tool contracts.
- For compatibility with older servers, the plugin sends both `projectRootPath` and `scenePath` payload keys for root-scoped physics commands.

## Camera Module (`camera.*`)
- `camera.list(projectRootPath)`
- `camera.create(scenePath, nodePath, cameraType, preset?)`
- `camera.update(scenePath, nodePath, properties)`
- `camera.validate(projectRootPath)`

Notes:
- The plugin exposes typed camera wrappers and SK camera skill methods for list/create/update/validate workflows.

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

## Lint Compatibility
- `lint_project(projectPath)`
- `lint.project_advanced(projectPath)`

Notes:
- The plugin exposes both advanced lint APIs and a basic project lint wrapper with fallback between `lint_project` and `lint.project_advanced` for cross-version server compatibility.

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
