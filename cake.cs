#!
#:sdk Cake.Sdk
#:property IncludeAdditionalFiles=cake/**/*.cs

var target = Argument<string>("target");

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

var publish = AddPublishTask()
    .IsDependentOn(build)
    .IsDependentOn(test)
    .IsDependentOn(pack);

var renovate = AddRenovateTask();

RunTarget(target);
