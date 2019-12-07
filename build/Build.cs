using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using Nuke.Docker;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

using static Nuke.Docker.DockerBuildSettings;
using static Nuke.Docker.DockerTasks;
using LogLevel = Nuke.Common.LogLevel;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion] readonly GitVersion GitVersion;

    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath PullRequestDockerFile => RootDirectory / "Dockerfile.pullrequest";
    AbsolutePath ReleaseDockerFile => RootDirectory / "Container.Deployed.Test/Dockerfile";
    AbsolutePath ReleaseProjectDirectory => RootDirectory / "Container.Deployed.Test";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(_ => _
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(_ => _
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .EnableNoRestore());
        });

    Target PullRequest => _ => _
        .Executes(() =>
        {
            Logger.Log(LogLevel.Warning, "Creating Docker Image...");

            DockerBuild(s => s
                .AddLabel("pull-request")
                .AddTag("pull-request")
                .SetFile(PullRequestDockerFile)
                .SetForceRm(true)
                .SetPath(RootDirectory)
            );
        });

    Target Release => _ => _
        .Executes(() =>
        {
            Logger.Log(LogLevel.Warning, "Creating Docker Image...");

            DockerBuild(s => s
                .AddLabel("release")
                .AddTag("localhost:5000/release")
                .SetFile(ReleaseDockerFile)
                .SetForceRm(true)
                .SetPath(ReleaseProjectDirectory)
            );

            DockerPush(settings => settings.SetName("localhost:5000/release"));
        });

}
