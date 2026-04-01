# SKILL: Godot MCP + Semantic Kernel Integration Guide

## Purpose
This guide defines how to use `GodotMcp.SemanticKernel.Plugin` correctly from both:
- **Human developers**
- **AI agents** (Copilot, autonomous coding agents, LLM workflows)

It standardizes responsibility boundaries, invocation patterns, safety rules, and expected implementation quality.

---

## Scope
Applies to projects that:
- target modern `.NET` (this repository targets `.NET 10`)
- use `Microsoft.SemanticKernel`
- use Godot Editor automation via MCP (`godot-mcp`)

## Exposed Tooling

In expanded mode, each discovered MCP method is exposed as a kernel function named `godot_<method>`.
Examples:
- `godot_create_scene(scenePath, rootNodeName, rootNodeType)`
- `godot_add_node(scenePath, parentPath, nodeName, nodeType)`
- `godot_create_script(path, language, baseType, className)`
- `godot_run_editor_command(arguments)`
- `godot_discover_integrations()`
- `godot_verify_integration_health(integrationName)`

---

## Core Mental Model
There are two valid execution modes:

1. **Manual invocation**
   - Client calls `GodotPlugin.InvokeToolAsync(toolName, parameters)` directly.
   - Deterministic and explicit.

2. **Automatic invocation (tool-calling)**
   - Client configures Semantic Kernel function calling (`FunctionChoiceBehavior.Auto()` or equivalent).
   - Model decides when to call Godot tools.

### Responsibility Boundary
- **Plugin responsibility**
  - connect to MCP server
  - discover/register Godot tools
  - validate/convert parameters
  - invoke tools safely and return results
- **Client responsibility**
  - host/kernel/model configuration
  - enabling automatic function calling
  - prompt/chat orchestration and execution policy

> The plugin exposes callable functions; the client decides orchestration mode.

---

## Required Runtime Preconditions
Before invoking tools:
1. `godot-mcp` is reachable by executable path.
2. `godot-mcp` executable is available.
3. `GodotPlugin.InitializeAsync()` has completed successfully.

If engine-backed tools are required, set `GODOT_PATH` or `GodotExecutablePath`.

---

## Standard Setup (Developer)
1. Register plugin services with `AddGodotMcp(...)`.
2. Resolve `GodotPlugin` from DI.
3. Call `InitializeAsync()`.
4. Choose invocation mode:
   - direct: `InvokeToolAsync(...)`
   - SK auto tool-calling: register plugin in kernel + enable function choice auto

---

## Standard Setup (AI Agent)
When generating code, follow this order:
1. Ensure DI registration exists (`AddGodotMcp(...)`).
2. Ensure plugin initialization is present and awaited.
3. If user asks for "automatic", configure SK execution settings for auto function calling in the **client**.
4. Keep manual path available for deterministic fallback.

Do not claim automatic invocation is plugin-only.

---

## Plugin Attribute Guidance
`GodotPlugin.InvokeToolAsync` is already suitable for SK exposure when decorated with:
- `KernelFunction`
- meaningful `Description` attributes on method + parameters

If adding new callable methods, include these attributes and clear parameter descriptions.

---

## Prompting Guidelines for AI Agents
When the task is Godot automation:
- Prefer explicit tool intent in prompts:
  - scene name
  - object names
  - positions/parameters
  - expected outcome
- Ask for structured outputs when possible.
- Keep prompts deterministic for CI/testing scenarios.

### Example Prompt Pattern
"Create a Godot scene named `DemoScene`, add a directional light, then return a short summary of created objects. Use available Godot tools."

---

## Safety and Validation Rules
- Never bypass plugin parameter validation.
- Never hardcode secrets/tokens in source.
- Treat tool inputs as untrusted; sanitize/log safely.
- Prefer typed/known parameter keys from discovered tool definitions.

---

## Error Handling Policy
Catch and classify known plugin exceptions:
- timeout
- network/transport
- protocol/server errors
- validation errors

Return actionable diagnostics:
- tool name
- high-level failure reason
- retry suggestion (if transient)

Do not expose sensitive internals in logs.

---

## Reliability Rules
- Use configurable timeouts.
- Use retry for transient failures only.
- Support cancellation tokens for long-running workflows.
- Keep `EnableMessageLogging` off in production unless troubleshooting.

---

## Testing Expectations
For any feature or change touching invocation:
1. unit test parameter validation/conversion behavior
2. unit test failure paths and exception mapping
3. integration test at least one end-to-end tool call
4. verify both manual and auto-call client flows (when relevant)

---

## Documentation Requirements
When adding new capability, update:
- quick start snippets
- auto-invocation snippet (if behavior changed)
- configuration table (if options changed)
- troubleshooting section for new failure modes

---

## Anti-Patterns
Avoid:
- assuming SK auto-calls tools without client function-calling config
- invoking tools before plugin initialization
- burying important runtime assumptions in code comments only
- writing examples that cannot compile as shown

---

## Definition of Done (for PRs and AI-generated changes)
A change is complete when:
- code compiles
- relevant tests pass
- README examples remain consistent with API
- responsibility boundary (plugin vs client) is clear
- no sensitive data exposure in logs/errors

---

## Quick Decision Table
| Goal | Use |
|---|---|
| Deterministic operation | `InvokeToolAsync` manual path |
| LLM decides tools dynamically | SK auto function calling in client |
| Multiple Godot endpoints/environments | keyed DI registrations |
| Production stability | manual fallback + retries + observability |

---

## Short Summary
- The plugin provides Godot MCP capabilities.
- The client controls orchestration and auto-calling behavior.
- Keep setup explicit, validated, testable, and secure.
