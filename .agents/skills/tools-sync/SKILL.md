---
name: tools-sync
description: Install or update the plugins, skills and MCP servers this repo declares in `.agents/plugins.json` and `.agents/mcp.json`, in the way your own agent supports. Use during repo setup, after template-sync changed those files, or when asked to install, update or check the repo's agent tools, plugins, skills or MCP servers.
---

# Tools sync

Manifests (neutral, not any agent's format):

- `.agents/plugins.json`: `plugins[]` with `name`, `type`, `repo`, `ref`, `paths`, optional `install`/`update` (command lists keyed by agent), optional `requires` (`mcp:<name>`).
  - `type: plugin`: a plugin from a marketplace. Use `install[<your agent>]`.
  - `type: skills`: plain skill folders. Copy `paths` from `repo` at `ref`.
- `.agents/mcp.json`: `servers.<name>` with `command`, `args`, optional `install` and `update` (shell commands the user runs), and notes (`requires`, `docs`). Stdio servers only.

## Rules

- Configure only yourself. You can reliably set up the agent you are; for any other agent, print the steps and stop.
- Plan first, then ask. Show what will be installed and where; wait for confirmation. Ask scope for MCP servers: user/local scope (default, nothing committed, keeps the repo agent-neutral) or a project file the user will commit.
- Never overwrite an existing entry with the same name that differs from the manifest. Show both and ask.
- Slash commands (`/plugin ...`) cannot be run by you. Hand them to the user verbatim. If your agent has a non-interactive CLI equivalent, use that instead.
- Do not commit. Do not put secrets in any file.
- Locations and commands below come from third-party sources and the README of each tool, not from agent vendors' docs. If a command or path fails or looks different in your version, check your agent's current docs or ask the user. Do not guess.

## Steps

1. **Identify your agent** (Claude Code, Codex, Cursor, Gemini CLI, Copilot/VS Code, OpenCode, other). If unsure, ask.
2. **Read both manifests.** Validate they parse. Missing file: skip that part.
3. **Check what is already there**, using your agent's own list command if it has one (for example `claude mcp list`, `codex plugin list`), else the config files below.
4. **Prerequisites.** For each server's `requires`, check it (for ".NET 10 SDK": `dotnet --list-sdks` shows a 10.x). Report missing ones; do not install SDKs. Then check the server's `command` is on `PATH` (`command -v` or `Get-Command`). If not and `install` is set, prompt the user to run it, show the exact command, and wait. Do not run it yourself.
5. **Plan.** One line per item: install, already present, update, skipped (reason).
6. **Plugins (`type: plugin`).** If `install` has an entry for your agent, run or hand over those commands. Otherwise fall back to copying `paths` (below) and say it is a plain skill copy, not a plugin.
7. **Skills (`type: skills`, or plugin fallback).** Shallow-clone `repo` at `ref` into a temp dir outside the repo. Copy each skill folder named by `paths` (a `*` means every folder under it) into your agent's skills directory, keeping the folder name equal to the `name` in its `SKILL.md`. Skip folders that already match; ask on differences.

   | Agent | Project skills dir | User skills dir |
   |---|---|---|
   | Claude Code | `.claude/skills/` | `~/.claude/skills/` |
   | Codex | `.agents/skills/` | `~/.agents/skills/` |
   | Gemini CLI | `.gemini/skills/` (also reads `.agents/skills/`) | `~/.gemini/skills/` |
   | Cursor | `.cursor/skills/` (also reads the Claude, Codex and `.agents` dirs) | `~/.cursor/skills/` |
   | Copilot | `.github/skills/` | `~/.copilot/skills/` |
   | OpenCode | `.opencode/skills/` | `~/.config/opencode/skills/` |

   Prefer the user dir unless the user wants skills committed to the repo.
8. **MCP servers.** Prefer your agent's own command (Claude Code: `claude mcp add <name> -- <command> <args...>`; check `codex mcp add` and `gemini mcp add` exist before using them). Otherwise write the server into the agent's config file:

   | Agent | Project file | Shape |
   |---|---|---|
   | Claude Code | `.mcp.json` | `mcpServers.<name>: {command, args}` |
   | Cursor | `.cursor/mcp.json` | `mcpServers.<name>: {command, args}` |
   | VS Code / Copilot | `.vscode/mcp.json` | `servers.<name>: {command, args}` |
   | Codex | `.codex/config.toml` (trusted projects) | `[mcp_servers.<name>]` with `command`, `args` |
   | Gemini CLI | `.gemini/settings.json` | `mcpServers.<name>: {command, args}` |

   Keep `args` exactly as in the manifest; for Roslynk a missing `stdio` makes the client hang. Register a server only after its `command` resolves on `PATH`.
9. **Update mode** (user asked to update): run `update[<your agent>]` for plugins; for copied skills re-clone and compare as in step 7; for MCP servers prompt the user to run the server's `update` command, as in step 4.
10. **Verify and report.** Use the list command or re-read the config. Say what needs a restart or a trust prompt in your agent. List: installed, already present, handed to the user, skipped.
