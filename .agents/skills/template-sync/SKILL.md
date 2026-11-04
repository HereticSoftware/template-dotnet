---
name: template-sync
description: Pull the latest changes from the template this repo was created from (editorconfig, build props, gitattributes, workflows, cake helpers, skills) without overwriting local decisions. Use when asked to sync, update or merge from the template, or to get the latest template files. Not for first-time setup (use template-initialize).
---

# Template sync

Three-way compare per managed file: base (template at the recorded commit), ours (working tree), theirs (template at latest). Reads `.agents/template.json`: `upstream`, `ref`, `base`, `sync` (globs).

## Rules

- Touch only paths matching `sync`. Never touch anything else, and never `.agents/template.json` except to bump `base`.
- Never overwrite a locally modified file without the user's decision.
- Never leave conflict markers in a file the user did not choose to resolve that way.
- Do not commit. Work on the current branch's working tree; suggest a branch such as `chore/template-sync`.
- Repo setup: `base` must be non-empty, else stop and point to `template-initialize`.

## Steps

1. **Clean tree.** `git status --porcelain` must show no changes in `sync` paths. If it does, ask the user to commit or stash.
2. **Fetch upstream to a temp dir outside the repo** (do not add a remote to the user's repo):
   ```
   git clone --filter=blob:none --no-checkout <upstream> <tmp>
   git -C <tmp> rev-parse origin/<ref>        # THEIRS sha
   ```
   If THEIRS equals `base`: report "already up to date" and stop. If `base` is not found in the clone, stop and ask the user for a valid base.
3. **List candidate files**: union of `git -C <tmp> ls-tree -r --name-only <base>`, the same for THEIRS, and local files, filtered by the `sync` globs (`*` does not cross `/`; `**` does).
4. **Classify each file** by blob id. Ours id: `git hash-object --path=<file> -- <file>` (applies line-ending normalization, so CRLF working copies compare correctly). Base and theirs ids: `git -C <tmp> rev-parse --verify -q <sha>:<file>`; empty output (exit 1) means the file does not exist there. Without `--verify -q` git echoes the argument on a miss.

   | ours vs base | theirs vs base | Action |
   |---|---|---|
   | same as theirs | | nothing |
   | unmodified | changed, added or deleted | apply theirs (write, create or delete) |
   | modified | unchanged | keep ours |
   | modified | changed | **merge** (below) |
   | added locally, not in base | added upstream, different | **conflict**: ask |
   | modified | deleted upstream | **conflict**: ask keep or delete |

5. **Merge.** Write base and theirs blobs to temp files with `git -C <tmp> show <sha>:<file>`. Write ours normalized: `id=$(git hash-object -w --path=<file> -- <file>)` then `git cat-file blob $id`. Run `git merge-file -p <ours> <base> <theirs>`:
   - exit 0: clean merge, write the output to the file.
   - exit above 0: conflicts. Do not write. Show the conflicting hunks and ask per file: keep ours, take theirs, or write with conflict markers for manual resolution. Default if unanswered: keep ours and list it as unresolved.
6. **Version lines.** In `global.json` and `Directory.Packages.props`, a conflict that is only a version value: keep ours (Renovate owns versions), unless the user says otherwise.
7. **Bump `base`** to THEIRS only if every file ended resolved (applied, merged, or the user chose to keep ours). If any file was deferred, leave `base` unchanged; re-running is safe because already-applied files compare as equal.
8. **Clean up** the temp dir. Report in four lists: applied, merged cleanly, kept (ours), unresolved. If this run changed `.agents/skills/template-sync`, say so. If it changed `.agents/plugins.json` or `.agents/mcp.json`, offer to run `.agents/skills/tools-sync/SKILL.md` to apply them.
