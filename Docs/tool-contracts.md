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
- `project.set_config_value(projectPath, key, value, section?)`
- `project.remove_config_key(projectPath, key, section?)`

Notes:
- The plugin exposes typed project wrappers and SK project skill methods for create/info/autoload/plugin/set_config/remove_config workflows.

## Scene and Node
- Scene graph tools use the enforced scene contract: `projectPath + /scenes/ + fileName`.
- `fileName` is scene-local and must end with `.tscn` (for example `mouse_test.tscn` or `ui/menu.tscn`).
- Missing scene files may be bootstrapped by the server before node/property operations using `root_type` (default `Node`).
- `scene.list_nodes(projectPath, fileName, root_type?)`
- `scene.add_node(projectPath, fileName, parentNodePath, nodeName, nodeType, root_type?)`
- `scene.remove_node(projectPath, fileName, nodePath, root_type?)`
- `scene.move_node(projectPath, fileName, nodePath, newParentPath, index?, root_type?)`
- `scene.rename_node(projectPath, fileName, nodePath, newName, root_type?)`
- `scene.get_node_properties(projectPath, fileName, nodePath, root_type?)`
- `scene.set_node_properties(projectPath, fileName, nodePath, properties, root_type?)`

## Scene Signal Connections (v1.10.1)
- `scene.connection_add(projectPath, fileName, nodePath, signal, targetNodePath, method, connected?, flags?, root_type?)`
- `scene.connection_remove(projectPath, fileName, nodePath, signal, targetNodePath, method, root_type?)`
- `scene.connection_info(projectPath, fileName, nodePath?, root_type?)`

Notes:
- The plugin exposes scene connection wrappers and SK skill methods for add/remove/query signal wiring workflows.

## Input Map (v1.10.1)
- `input.list_actions(projectPath)`
- `input.get_action(projectPath, actionName)`
- `input.add_action(projectPath, actionName, deadzone?)`
- `input.remove_action(projectPath, actionName)`
- `input.add_input_event(projectPath, actionName, device, buttonMask?, keycode?, physicalKeycode?, gravity?, positionX?, positionY?, axis?)`
- `input.remove_input_event(projectPath, actionName, eventIndex)`

Notes:
- Device type values: `Joypad`, `Keyboard`, `Mouse`.
- The plugin exposes typed input map wrappers and SK input skill methods for action management.

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
- `resource.list(projectPath, directory?, resourceType?)` (v1.10.1+, fallback to `list_resources`)
- `resource.read(path)`
- `resource.update_properties(path, properties)`
- `resource.assign_texture(resourcePath, propertyPath, texturePath)` (v1.10.1)
- `generate_import_file(assetPath, importer, type, parameters?)`
- `reimport_asset(assetPath)`
- `create_texture(texturePath)`
- `create_audio(audioPath)`

Notes:
- The plugin exposes typed import wrappers and SK import skill methods for import file generation and asset reimport workflows.
- The plugin resource typed wrappers are compatibility-aware and can use either modern (`create_resource`, `resource.update_properties`) or legacy (`resource.create`, `resource.update`) command names based on server support.
- `resource.list` has fallback to `list_resources` for cross-version compatibility.

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
- `physics.remove_shape(scenePath, shapePath)` (v1.10.1)
- `physics.add_collision_polygon(scenePath, bodyPath, polygonName, points)` (v1.10.1)
- `physics.update_collision_polygon(scenePath, polygonPath, points?, properties?)` (v1.10.1)
- `physics.remove_collision_polygon(scenePath, polygonPath)` (v1.10.1)
- `physics.assign_shape_resource(scenePath, shapePath, resourcePath)` (v1.10.1)
- `physics.set_shape_flags(scenePath, shapePath, disabled?, trigger?)` (v1.10.1)
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

Note: Starting with GodotMCP.Server v1.7.1 the MCP SDK may include a `RawContent`
field on tool results containing either structured JSON, a text block, or an
enumerable of content blocks. This plugin detects and parses `RawContent`
transparently (falling back to legacy `Content`/`StructuredContent` handling), so
servers using the newer contract remain compatible.

## Generated Content

LLMs commonly generate multi-line text or source code that must be forwarded to
Godot via MCP tool parameters. The plugin preserves and forwards generated
content parameters verbatim after parameter conversion:

- Prefer using parameter names such as `rawContent` or `content` in tool
  definitions when the parameter is expected to contain LLM-generated text.
- Simple string values are forwarded as plain strings. Complex objects are
  serialized to `JsonElement` (using the project's `McpJsonSerializerContext`) so
  they can be transported safely via MCP and reconstituted by the server.

Example: When invoking `create_script(rawContent)` the plugin will pass the
string value generated by the LLM directly to the MCP tool without trimming or
modification (after conversion), ensuring the server receives the full script
text.
