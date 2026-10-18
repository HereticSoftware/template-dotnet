---
name: template-initialize
description: Initialize a repository created from a template. Use right after creating a repo from the template, or when asked to set up, initialize or bootstrap a new repo, fill template placeholders, or set repository name, description, SDK and package versions. Not for pulling later template updates (use template-sync).
---

# Template initialize

Fills placeholders with the user's answers and current versions, then records the template commit this repo derives from. Reads `.agents/template.json`.

## Rules

- Ask before writing. Never guess names, descriptions, licenses or the year.
- Never take versions from memory. Query a live source (below) and show the user what was found.
- Do not commit. Leave changes in the working tree.
- Do not invent binary files (icons, logos).

## Steps

1. **Preconditions.** Inside a git repo. `.agents/template.json` exists. If `base` is non-empty the repo is already initialized: stop and point to `template-sync`.
2. **Ask the user** (offer defaults in brackets):
   - Repository name [current folder name]
   - Description, tags (semicolon separated)
   - Authors, organization URL [current values in `Directory.Build.props`]
   - License expression [current value]
   - Copyright year [current year]
3. **Fill `Directory.Build.props`**: `RepositoryName`, `RepositoryDescription`, `RepositoryTags`, `Year`, plus `Authors`, `OrganizationUrl`, `RepositoryLicense` if changed. Edit values only; keep structure and comments.
4. **Resolve versions.** Find every empty version: `Version=""` in `Directory.Packages.props`, `sdk.version` and `msbuild-sdks.*` in `global.json`. For each:
   - NuGet package or MSBuild SDK: `GET https://api.nuget.org/v3-flatcontainer/<lowercase-id>/index.json`, take the highest version without a `-` suffix.
   - .NET SDK: highest stable from `dotnet --list-sdks` as the default; also show the latest stable from `https://builds.dotnet.microsoft.com/dotnet/release-metadata/releases-index.json` (`latest-sdk` of the newest supported channel). Ask which to pin. The pinned SDK must be installed locally.
   - Show a table (item, version, source) and get confirmation before writing.
5. **Referenced files.** `Directory.Build.props` packs `README.md` and `Icon.png` from the repo root.
   - Missing `README.md`: create a stub with the repository name as title and the description.
   - Missing `Icon.png`: tell the user to supply one, or offer to remove the `PackageIcon` and `Icon.png` entries. Do not fabricate it.
   - Offer a `LICENSE` file matching the license and year. Skip if declined.
6. **Workflows.** List what `.github/workflows/*` needs from the user and do not set it: secrets (for example `NUGET_USER`), and any `uses:` pointing at another organization's repo or action.
7. **Record the base.** Set `base` in `.agents/template.json` to `git ls-remote <upstream> <ref>` (the commit hash). If the repo was copied from an older template state, ask which tag or commit it started from and use that.
8. **Verify.** Run `dotnet restore` (or the repo's Cake `Restore` target) and report the result. Do not hide failures.
9. **Report.** List files changed and values chosen. Suggest `git add` and a commit; do not run them without being asked.
