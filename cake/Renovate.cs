using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Core;
using Cake.Core.IO;

// dotnet cake.cs --target=renovate --platform=github --repository=owner/repo [--dry-run]
public static class RenovateExtensions
{
    /// <summary>
    /// Runs Renovate against <paramref name="repository"/> on <paramref name="platform"/> (github, gitlab, ...) via <c>npx renovate</c>.
    /// Auth: <c>RENOVATE_TOKEN</c> environment variable (read by Renovate itself).
    /// <paramref name="dryRun"/>: <c>--platform=local</c>, no PRs, no token, works without a Renovate config
    /// (<c>--onboarding=false --require-config=optional</c>, avoids the onboarding flow). Sets <c>LOG_LEVEL=debug</c> unless already set,
    /// since proposed updates are only logged at debug level.
    /// Throws on non-zero exit code.
    /// </summary>
    public static void Renovate(this ICakeContext context, string repository, string platform, bool dryRun)
    {
        var args = new ProcessArgumentBuilder().Append("renovate");
        if (dryRun)
        {
            args.Append("--platform=local").Append("--onboarding=false").Append("--require-config=optional");
        }
        else
        {
            args.Append($"--platform={platform}").Append(repository);
        }

        var npxName = context.Environment.Platform.Family == PlatformFamily.Windows ? "npx.cmd" : "npx";
        var npx = context.Tools.Resolve(npxName) ?? throw new CakeException($"`{npxName}` was not found on PATH");
        context.Information("{0} {1}", npx.GetFilename(), args.RenderSafe());
        var settings = new ProcessSettings { Arguments = args };
        if (dryRun && string.IsNullOrEmpty(context.EnvironmentVariable("LOG_LEVEL", string.Empty)))
        {
            settings.EnvironmentVariables = new Dictionary<string, string> { ["LOG_LEVEL"] = "debug" };
        }

        var exitCode = context.StartProcess(npx, settings);
        if (exitCode != 0)
        {
            throw new CakeException($"Renovate failed with exit code {exitCode}");
        }
    }
}

public static partial class Program
{
    /// <summary>
    /// Registers a task running <see cref="RenovateExtensions.Renovate"/>.
    /// Arguments: <c>--repository=owner/repo</c>, <c>--platform=github|gitlab|...</c> (else <c>RENOVATE_PLATFORM</c>), <c>--dry-run</c> (flag).
    /// Platform, repository and <c>RENOVATE_TOKEN</c> are required unless <c>--dry-run</c>.
    /// </summary>
    public static CakeTaskBuilder AddRenovateTask(string name = "Renovate")
    {
        var repository = Argument<string>("repository", string.Empty);
        var platform = Argument<string>("platform", EnvironmentVariable("RENOVATE_PLATFORM", string.Empty));
        var dryRun = HasArgument("dry-run");
        var token = EnvironmentVariable("RENOVATE_TOKEN", string.Empty);

        return Task(name)
            .WithCriteria(() => dryRun || !string.IsNullOrEmpty(platform), "Argument `--platform` or environment variable `RENOVATE_PLATFORM` was not provided")
            .WithCriteria(() => dryRun || !string.IsNullOrEmpty(token), "Environment variable `RENOVATE_TOKEN` was not provided")
            .WithCriteria(() => dryRun || !string.IsNullOrEmpty(repository), "Argument `--repository` was not provided")
            .Does(context => context.Renovate(repository, platform, dryRun));
    }
}
