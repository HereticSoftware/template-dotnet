---
name: template-apply
description: Apply the template this repo derives from. First run (empty `base`) sets up the repo, filling placeholders and versions; later runs pull template changes (editorconfig, build props, gitattributes, workflows, cake helpers, skills) without overwriting local decisions. Use when asked to set up, initialize or bootstrap a repo from the template, fill template placeholders, or sync, update or merge from the template. Not for installing agent tools (use tools-sync).
---

# Template apply

Three-way compare per managed file: base (template at the recorded commit), ours (working tree), theirs (template at latest). Reads `.agents/template.json`: `upstream`, `ref`, `base`, `sync` (globs).

Mode: `base` empty is **init** (first run on a new repo); otherwise **update**. Steps marked *init* run only in init mode.

## Rules

- Touch only paths matching `sync`, plus in init mode the files named in steps 2 and 9. Never touch `.agents/template.json` except to set `base`.
- Ask before writing. Never guess names, descriptions, licenses or the year.
- Never take versions from memory. Query a live source (step 8) and show the user what was found.
- Never overwrite a locally modified file without the user's decision.
- Never leave conflict markers in a file the user did not choose to resolve that way.
- Do not commit. Work on the current branch's working tree; in update mode suggest a branch such as `chore/template-apply`.
- Do not invent binary files (icons, logos).

## Steps

1. **Preconditions.** Inside a git repo. `.agents/template.json` exists. `git status --porcelain` shows no changes in `sync` paths; if it does, ask the user to commit or stash. Determine the mode.
2. *init* **Ask the user** (offer defaults in brackets), then fill `Directory.Build.props`: `RepositoryName`, `RepositoryDescription`, `RepositoryTags`, `Year`, plus `Authors`, `OrganizationUrl`, `RepositoryLicense` if changed. Edit values only; keep structure and comments.
   - Repository name [current folder name]
   - Description, tags (semicolon separated)
   - Authors, organization URL [current values in `Directory.Build.props`]
   - License expression [current value]
   - Copyright year [current year]
3. **Fetch upstream to a temp dir outside the repo** (do not add a remote to the user's repo):
   ```
   git clone --filter=blob:none --no-checkout <upstream> <tmp>
   git -C <tmp> rev-parse origin/<ref>        # THEIRS sha
   ```
   - *update*: if THEIRS equals `base`, report "already up to date" and go to step 8. If `base` is not found in the clone, stop and ask the user for a valid base.
   - *init*: base defaults to THEIRS. Ask whether the repo was created from an older template state; if so, use that tag or commit as base. If base equals THEIRS, go to step 8.
4. **List candidate files**: union of `git -C <tmp> ls-tree -r --name-only <base>`, the same for THEIRS, and local files, filtered by the `sync` globs (`*` does not cross `/`; `**` does).
5. **Classify each file** by blob id. Ours id: `git hash-object --path=<file> -- <file>` (applies line-ending normalization, so CRLF working copies compare correctly). Base and theirs ids: `git -C <tmp> rev-parse --verify -q <sha>:<file>`; empty output (exit 1) means the file does not exist there. Without `--verify -q` git echoes the argument on a miss.

   | ours vs base | theirs vs base | Action |
   |---|---|---|
   | same as theirs | | nothing |
   | unmodified | changed, added or deleted | apply theirs (write, create or delete) |
   | modified | unchanged | keep ours |
   | modified | changed | **merge** (step 6) |
   | added locally, not in base | added upstream, different | **conflict**: ask |
   | modified | deleted upstream | **conflict**: ask keep or delete |

6. **Merge.** Write base and theirs blobs to temp files with `git -C <tmp> show <sha>:<file>`. Write ours normalized: `id=$(git hash-object -w --path=<file> -- <file>)` then `git cat-file blob $id`. Run `git merge-file -p <ours> <base> <theirs>`:
   - exit 0: clean merge, write the output to the file.
   - exit above 0: conflicts. Do not write. Show the conflicting hunks and ask per file: keep ours, take theirs, or write with conflict markers for manual resolution. Default if unanswered: keep ours and list it as unresolved.
7. **Version conflicts.** In `global.json` and `Directory.Packages.props`, a conflict that is only a version value: keep ours (Renovate owns versions), unless the user says otherwise.
8. **Resolve empty versions** (both modes; the template keeps versions empty, so merges can bring new ones). Find every `Version=""` in `Directory.Packages.props` files and empty `sdk.version` / `msbuild-sdks.*` in `global.json`. For each:
   - NuGet package or MSBuild SDK: `GET https://api.nuget.org/v3-flatcontainer/<lowercase-id>/index.json`, take the highest version without a `-` suffix.
   - .NET SDK: highest stable from `dotnet --list-sdks` as the default; also show the latest stable from `https://builds.dotnet.microsoft.com/dotnet/release-metadata/releases-index.json` (`latest-sdk` of the newest supported channel). Ask which to pin. The pinned SDK must be installed locally.
   - Show a table (item, version, source) and get confirmation before writing.
9. *init* **Referenced files.** `Directory.Build.props` packs `README.md` and `Icon.png` from the repo root.
   - Missing `README.md`: create a stub with the repository name as title and the description.
   - Missing `Icon.png`: tell the user to supply one, or offer to remove the `PackageIcon` and `Icon.png` entries. Do not fabricate it.
   - Offer a `LICENSE` file matching the license and year. Skip if declined.
10. **Workflows.** If this run created or changed `.github/workflows/*` (always in init mode), list what they need from the user and do not set it: secrets (for example `NUGET_USER`), and any `uses:` pointing at another organization's repo or action.
11. **Set `base`** to THEIRS only if every file ended resolved (applied, merged, or the user chose to keep ours). If any file was deferred, leave `base` unchanged; re-running is safe because already-applied files compare as equal.
12. **Verify.** Run `dotnet restore` (or the repo's Cake `Restore` target) and report the result. Do not hide failures.
13. **Clean up** the temp dir. Report: values chosen (init), versions written, and files in four lists: applied, merged cleanly, kept (ours), unresolved. If this run changed `.agents/skills/template-apply`, say so. Suggest `git add` and a commit; do not run them without being asked.
14. **Agent tools.** In init mode, or if this run changed `.agents/plugins.json` or `.agents/mcp.json`, offer to run `.agents/skills/tools-sync/SKILL.md`. Skip if declined.
