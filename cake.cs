#!
#:sdk Cake.Sdk

var target = Argument<string>("target");
var nugetApiKey = EnvironmentVariable("NUGET_API_KEY", string.Empty);
var nugetSource = EnvironmentVariable("NUGET_SOURCE", string.Empty);

var configuration = "Release";
var version = ThisAssembly.Info.InformationalVersion;
Information("Version: v{0}, Configuration: {1}", version, configuration);

DirectoryPath[] srcProjects = [
    "src/",
];
DirectoryPath[] testProjects = [
    "test/",
];
DirectoryPath[] projects = [
    .. srcProjects,
    .. testProjects
];

var restore = Task("Restore")
    .DoesForEach(projects, dir =>
    {
        Information("\nRestore {0}", dir.GetDirectoryName());
        DotNetRestore(dir.FullPath);
    });

var build = Task("Build")
    .DoesForEach(projects, dir =>
    {
        Information("\nBuild {0}", dir.GetDirectoryName());
        DotNetBuild(dir.FullPath, new() { Configuration = configuration });
    });

var pack = Task("Pack")
    .IsDependentOn(build)
    .Does(() => CleanDirectory("packages"))
    .DoesForEach(srcProjects, file =>
        DotNetPack(file.FullPath, new() { NoRestore = true, NoBuild = true })
    );

var test = Task("Test")
    .DoesForEach(testProjects, dir =>
    {
        Information("\nTest {0}", dir.GetDirectoryName());
        DotNetTest(dir.FullPath, new() { PathType = DotNetTestPathType.Project, Configuration = configuration });
    });

var pullRequest = Task("Pull-Request")
    .IsDependentOn(build)
    .IsDependentOn(test);

var publish = Task("Publish")
    .WithCriteria(!string.IsNullOrEmpty(nugetSource), "Environment variable `NUGET_API_KEY` was not provided")
    .WithCriteria(!string.IsNullOrEmpty(nugetApiKey), "Environment variable `NUGET_SOURCE` was not provided")
    .IsDependentOn(build)
    .IsDependentOn(test)
    .IsDependentOn(pack)
    .WithCriteria(() => GetFiles("packages/*.nupkg").Count != 0, "No packages were produced")
    .Does(() =>
    {
        Information("NuGet Push");
        DotNetNuGetPush("packages/*.nupkg", new() { Source = nugetSource, ApiKey = nugetApiKey });
    });

RunTarget(target);
