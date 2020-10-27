using System.Linq;
using Nuke.Common;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.Octopus;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Octopus.OctopusTasks;

[GitHubActions(
    "continuous",
    GitHubActionsImage.WindowsLatest,
    On = new[] {GitHubActionsTrigger.Push},
    InvokedTargets = new[] {nameof(DeployRelease)},
    ImportSecrets = new[] {nameof(OctopusServer), nameof(OctopusApiKey)})]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.DeployRelease);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [GitVersion] readonly GitVersion GitVersion;

    [Parameter] readonly string DatabaseRuntime = "win-x64";

    [Parameter] readonly string OctopusServer;
    [Parameter] readonly string OctopusApiKey;
    [Parameter] readonly string OctopusSpace = "Default";
    [Parameter] readonly string OctopusProject = "OctoPetShop";
    [Parameter] readonly string OctopusEnvironment = "Development";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(_ => _
                .CombineWith(Solution.AllProjects, (_, v) => _
                    .SetProjectFile(v.Path)
                    .When(v.Name == "OctopusSamples.OctoPetShop.Database", _ => _
                        .SetRuntime(DatabaseRuntime))));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(_ => _
                .SetConfiguration(Configuration)
                .EnableNoRestore()
                .CombineWith(Solution.AllProjects, (_, v) => _
                    .SetProjectFile(v.Path)
                    .When(v.Name == "OctopusSamples.OctoPetShop.Database", _ => _
                        .SetRuntime(DatabaseRuntime))));
        });

    AbsolutePath TestResultsDirectory => RootDirectory / "testResults";

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(_ => _
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .SetResultsDirectory(TestResultsDirectory)
                .CombineWith(Solution.AllProjects.Where(x => x.Name.EndsWith(".Tests")), (_, v) => _
                    .SetProjectFile(v)
                    .SetLogger($"trx;LogFileName={v.Name}.trx")));

            AzurePipelines.Instance?.PublishTestResults(
                title: "Test Results",
                type: AzurePipelinesTestResultsType.XUnit,
                files: TestResultsDirectory.GlobFiles("*.trx").Select(x => x.ToString()));
        });

    AbsolutePath PublishDirectory => RootDirectory / "publish";

    Target Publish => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetPublish(_ => _
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .CombineWith(Solution.AllProjects.Where(x => !x.Name.EndsWith(".Tests")), (_, v) => _
                    .SetProject(v.Path)
                    .SetOutput(PublishDirectory / v.Name)
                    .When(v.Name == "OctopusSamples.OctoPetShop.Database", _ => _
                        .SetRuntime(DatabaseRuntime))));

            CopyDirectoryRecursively(
                source: RootDirectory / "OctopusSamples.OctoPetShop.Infrastructure",
                target: PublishDirectory / "OctopusSamples.OctoPetShop.Infrastructure");
        });

    AbsolutePath PackageDirectory => RootDirectory / "package";

    Target Pack => _ => _
        .DependsOn(Publish)
        .Executes(() =>
        {
            var packageIds = Solution.AllProjects.Where(x => !x.Name.EndsWith(".Tests")).Select(x => x.Name)
                .Concat("OctopusSamples.OctoPetShop.Infrastructure");

            OctopusPack(_ => _
                .SetFormat(OctopusPackFormat.NuPkg)
                .SetOutputFolder(PackageDirectory)
                .SetVersion(GitVersion.NuGetVersionV2)
                .CombineWith(packageIds, (_, v) => _
                    .SetId(v)
                    .SetBasePath(PublishDirectory / v)));
        });

    Target Push => _ => _
        .DependsOn(Pack)
        .Executes(() =>
        {
            OctopusPush(_ => _
                .SetServer(OctopusServer)
                .SetApiKey(OctopusApiKey)
                .SetSpace(OctopusSpace)
                .SetPackage(PackageDirectory.GlobFiles("*.nupkg").Select(x => x.ToString())));
        });

    Target CreateRelease => _ => _
        .DependsOn(Push)
        .Executes(() =>
        {
            OctopusCreateRelease(_ => _
                .SetServer(OctopusServer)
                .SetApiKey(OctopusApiKey)
                .SetSpace(OctopusSpace)
                .SetDefaultPackageVersion(GitVersion.NuGetVersionV2)
                .SetVersion(GitVersion.NuGetVersionV2));
        });

    Target DeployRelease => _ => _
        .DependsOn(CreateRelease)
        .Requires(() => OctopusServer)
        .Requires(() => OctopusApiKey)
        .Executes(() =>
        {
            OctopusDeployRelease(_ => _
                .SetServer(OctopusServer)
                .SetApiKey(OctopusApiKey)
                .SetSpace(OctopusSpace)
                .SetProject(OctopusProject)
                .SetDeployTo(OctopusEnvironment)
                .SetVersion(GitVersion.NuGetVersionV2)
                .EnableProgress());
        });
}
