public static partial class Program
{
    /// <summary>
    /// Registers a task pushing <paramref name="packages"/> with <c>dotnet nuget push</c>.
    /// Source: <c>--nuget-source</c> argument, else <c>NUGET_SOURCE</c> environment variable.
    /// API key: <c>NUGET_API_KEY</c> environment variable only (never an argument, would leak into logs and shell history).
    /// Skipped if source or key is missing, or <paramref name="packages"/> matches no files.
    /// Dependencies (build, test, pack) are added by the caller via <c>IsDependentOn</c>.
    /// </summary>
    public static CakeTaskBuilder AddNugetPush(string name = "Nuget-Push", string packages = "packages/*.nupkg")
    {
        var source = Argument<string>("nuget-source", EnvironmentVariable("NUGET_SOURCE", string.Empty));
        var apiKey = EnvironmentVariable("NUGET_API_KEY", string.Empty);

        return Task(name)
            .WithCriteria(() => !string.IsNullOrEmpty(source), "Argument `--nuget-source` or environment variable `NUGET_SOURCE` was not provided")
            .WithCriteria(() => !string.IsNullOrEmpty(apiKey), "Environment variable `NUGET_API_KEY` was not provided")
            .WithCriteria(() => GetFiles(packages).Count != 0, "No packages were produced")
            .Does(() => DotNetNuGetPush(packages, new() { Source = source, ApiKey = apiKey }));
    }
}
