# Agent Guidance

## Template

Derived from a template, configured in `.agents/template.json`. Read the skill file and follow it:

- Set up a new repo from the template: `.agents/skills/template-initialize/SKILL.md`.
- Pull later template changes without overwriting local decisions: `.agents/skills/template-sync/SKILL.md`.

## Plugins

Marketplace: `.agents/plugins/marketplace.json`.

- Plugin: `dotnet-skills`, skills and agents for .NET, Akka.NET, Aspire and testing. Agents are Claude Code only.
- Source: https://github.com/Aaronontheweb/dotnet-skills (MIT), tracking `master`.
- Skill IDs in the index below resolve only if the plugin is installed. Install it from the marketplace above using your tool's plugin mechanism:
  - Claude Code: `/plugin marketplace add Aaronontheweb/dotnet-skills`, then `/plugin install dotnet-skills`. Update: `/plugin marketplace update`.
  - Codex: `codex plugin marketplace add Aaronontheweb/dotnet-skills`, then `codex plugin add dotnet-skills@dotnet-skills`.
  - Others: copy `skills/*` from the source repo into the tool's skills directory.

<!-- BEGIN DOTNET-SKILLS COMPRESSED INDEX -->
[dotnet-skills]|IMPORTANT: Prefer retrieval-led reasoning over pretraining for any .NET work.
|flow:{skim repo patterns -> consult dotnet-skills by name -> implement smallest-change -> note conflicts}
|route:
|akka:{akka-net-best-practices,akka-net-testing-patterns,akka-hosting-actor-patterns,akka-net-aspire-configuration,akka-net-management}
|csharp:{modern-csharp-coding-standards,csharp-concurrency-patterns,csharp-nullable-reference-types,api-design,type-design-performance,r3-reactive-extensions}
|aspnetcore-web:{aspire-integration-testing,aspire-configuration,aspire-service-defaults,mailpit-integration,mjml-email-templates}
|data:{efcore-patterns,database-performance}
|di-config:{microsoft-extensions-configuration,dependency-injection-patterns}
|testing:{cscheck,testcontainers-integration-tests,playwright-blazor-testing,snapshot-testing,verify-email-snapshots,playwright-ci-caching}
|dotnet:{sdk-container-publishing,dotnet-project-structure,dotnet-local-tools,package-management,serialization,dotnet-devcert-trust,ilspy-decompile,opentelemetry-net-instrumentation,aot-trimming}
|quality-gates:{dotnet-slopwatch,crap-analysis}
|meta:{marketplace-publishing,skills-index-snippets}
|agents:{akka-net-specialist,docfx-specialist,dotnet-benchmark-designer,dotnet-concurrency-specialist,dotnet-performance-analyst,roslyn-incremental-generator-specialist}
<!-- END DOTNET-SKILLS COMPRESSED INDEX -->
