using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Core;
using Cake.Core.IO;

public static class NpmExtensions
{
    /// <summary>
    /// Runs <c>npm</c> with <paramref name="arguments"/> in <paramref name="workingDirectory"/>.
    /// Killed if the Cake process exits (.NET 11 <c>KillOnParentExit</c>).
    /// Throws on non-zero exit code.
    /// </summary>
    public static void Npm(this ICakeContext context, DirectoryPath workingDirectory, params string[] arguments)
    {
        var args = new ProcessArgumentBuilder();
        foreach (var argument in arguments)
        {
            args.Append(argument);
        }

        var npmName = context.Environment.Platform.Family == PlatformFamily.Windows ? "npm.cmd" : "npm";
        var npm = context.Tools.Resolve(npmName) ?? throw new CakeException($"`{npmName}` was not found on PATH");
        context.Information("{0} {1}", npm.GetFilename(), args.RenderSafe());

        var settings = new ProcessSettings { Arguments = args, WorkingDirectory = workingDirectory, KillOnParentExit = true };
        var exitCode = context.StartProcess(npm, settings);
        if (exitCode != 0)
        {
            throw new CakeException($"npm {args.RenderSafe()} failed with exit code {exitCode}");
        }
    }
}
