# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]
- Initial repository modernization: add CI, release workflow, license, and basic docs.
 - Adapt `StdioMcpClient` to handle `RawContent` in MCP responses (GodotMCP.Server v1.7.1 compatibility). Added unit tests and documentation updates.

## [1.8.0] - 2026-05-04
### Added
- **Project config**: Add `set_project_config` and `remove_project_config` for modifying project.godot settings
- **Resource**: Add `assign_texture` for assigning textures to resource properties; add `resource.list` with fallback to `list_resources` for v1.10.1 compatibility
- **Physics**: Add `remove_shape`, `add_collision_polygon`, `update_collision_polygon`, `remove_collision_polygon`, `assign_shape_resource`, `set_shape_flags` for v1.10.1 physics tooling
- **Scene connections**: Add `add_connection`, `remove_connection`, `list_connections` for node signal wiring
- **Input map**: Add full `InputMapSkill` with list_actions, get_action, add_action, remove_action, add_input_event, remove_input_event
- **Scene move index**: Fix bug where `scene.move_node` was not sending optional `index` parameter

### Changed
- Updated tool-contracts.md documentation with all v1.10.1 tool additions
