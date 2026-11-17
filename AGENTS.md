# Agent Guidance

## Template

Derived from a template, configured in `.agents/template.json`. Read the skill file and follow it:

- Set up a new repo from the template, or pull later template changes without overwriting local decisions: `.agents/skills/template-apply/SKILL.md`.
- Install or update the tools below for your agent: `.agents/skills/tools-sync/SKILL.md`.

## Tools

Declared in `.agents/plugins.json` (plugins and skills) and `.agents/mcp.json` (MCP servers). Follow `tools-sync` to install them; do not hand-edit agent config.

- `dotnet-skills` (plugin): .NET, Akka.NET, Aspire and testing skills. Source: https://github.com/Aaronontheweb/dotnet-skills (MIT). Agents it ships are Claude Code only.
- `roslynk` (MCP server and skill): semantic C# navigation, diagnostics and refactoring through Roslyn. Source: https://github.com/mrpmorris/Roslynk (MIT). Needs the .NET 10 SDK and a one-time user install: `dotnet tool install roslynk -g`.
- `pennington` (skill): docs sites and blogs with Pennington. Source: https://github.com/usepennington/skills (MIT). Beck diagrams have no skill; agents read https://usepennington.github.io/beck/for-ai.md.
- Skill IDs in the index below resolve only if `dotnet-skills` is installed.

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
