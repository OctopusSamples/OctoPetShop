#tool "nuget:?package=OctopusTools&version=6.13.1"

using Cake.Common.Tools.OctopusDeploy;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var version = Argument("packageVersion", "0.0.1");
var prerelease = Argument("prerelease", "");
var databaseRuntime = Argument("databaseRuntime", "win-x64");
var octopusServer = Argument("octopusServer", "https://your.octopus.server");
var octopusApiKey = Argument("octopusApiKey", "hey, don't commit your API key");
var octopusSpace = Argument("octopusSpace", "Default");
var octopusProject = Argument("octopusProject", "OctoPetShop");
var octopusEnvironment = Argument("octopusEnvironment", "Development");

class ProjectInformation
{
    public string Name { get; set; }
    public string FullPath { get; set; }
    public string Runtime { get; set; }
    public bool IsTestProject { get; set; }
}

string packageVersion;
List<ProjectInformation> projects;

Setup(context =>
{
    if (BuildSystem.IsLocalBuild && string.IsNullOrEmpty(prerelease))
    {
        prerelease = "-local";
    }

    packageVersion = $"{version}{prerelease}";

    projects = GetFiles("./**/*.csproj").Select(p => new ProjectInformation
    {
        Name = p.GetFilenameWithoutExtension().ToString(),
        FullPath = p.GetDirectory().FullPath,
        Runtime = p.GetFilenameWithoutExtension().ToString() == "OctopusSamples.OctoPetShop.Database" ? databaseRuntime : null,
        IsTestProject = p.GetFilenameWithoutExtension().ToString().EndsWith(".Tests")
    }).ToList();

    Information("Building OctoPetShop v{0}", packageVersion);
});

Task("Clean")
    .Does(() =>
        {
            CleanDirectory("publish");
            CleanDirectory("package");
            CleanDirectory("testResults");

            var cleanSettings = new DotNetCoreCleanSettings { Configuration = configuration };

            foreach(var project in projects)
            {
                DotNetCoreClean(project.FullPath, cleanSettings);
            }
        });

// Run dotnet restore to restore all package references.
Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        foreach(var project in projects)
        {
            var restoreSettings = new DotNetCoreRestoreSettings();

            if (!string.IsNullOrEmpty(project.Runtime))
            {
                restoreSettings.Runtime = project.Runtime;
            }

            DotNetCoreRestore(project.FullPath, restoreSettings);
        }
    });

 Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        foreach(var project in projects)
        {
            var buildSettings = new DotNetCoreBuildSettings()
                {
                    Configuration = configuration,
                    NoRestore = true
                };

            if (!string.IsNullOrEmpty(project.Runtime))
            {
                buildSettings.Runtime = project.Runtime;
            }

            DotNetCoreBuild(project.FullPath, buildSettings);
        }
    });

Task("RunUnitTests")
    .IsDependentOn("Build")
    .Does(() =>
    {
        foreach(var project in projects.Where(p => p.IsTestProject))
        {
            DotNetCoreTest(project.FullPath, new DotNetCoreTestSettings
                {
                    Configuration = configuration,
                    NoRestore = true,
                    NoBuild = true,
                    Logger = "TRX",
                    ResultsDirectory = "testResults"
                });
        }

        var isTFSBuild = TFBuild.IsRunningOnAzurePipelinesHosted;

        if (isTFSBuild)
        {
            TFBuild.Commands.PublishTestResults(new TFBuildPublishTestResultsData
                {
                    TestRunner = TFTestRunnerType.XUnit,
                    TestResultsFiles = GetFiles("testResults/*.trx").ToList()
                });
        }
    });

Task("Publish")
    .IsDependentOn("RunUnitTests")
    .Does(() =>
    {
        foreach(var project in projects.Where(p => !p.IsTestProject))
        {
            var publishSettings = new DotNetCorePublishSettings()
                {
                    Configuration = configuration,
                    OutputDirectory = System.IO.Path.Combine("publish", project.Name),
                    ArgumentCustomization = args => args.Append("--no-restore")
                };

            if (!string.IsNullOrEmpty(project.Runtime))
            {
                publishSettings.Runtime = project.Runtime;
            }

            DotNetCorePublish(project.FullPath, publishSettings);
        }

        // publish infrastructure
        CopyDirectory("OctopusSamples.OctoPetShop.Infrastructure", System.IO.Path.Combine("publish", "OctopusSamples.OctoPetShop.Infrastructure"));
    });

Task("Pack")
    .IsDependentOn("Publish")
    .Does(() =>
    {
        foreach(var project in projects.Where(p => !p.IsTestProject))
        {
            OctoPack(
                project.Name,
                new OctopusPackSettings()
                {
                    Format = OctopusPackFormat.NuPkg,
                    BasePath = System.IO.Path.Combine("publish", project.Name),
                    OutFolder = "package",
                    Version = packageVersion
                });
        }

        // pack infrastructure
        OctoPack(
            "OctopusSamples.OctoPetShop.Infrastructure",
            new OctopusPackSettings()
            {
                Format = OctopusPackFormat.NuPkg,
                BasePath = System.IO.Path.Combine("publish", "OctopusSamples.OctoPetShop.Infrastructure"),
                OutFolder = "package",
                Version = packageVersion
            });
    });

Task("PushPackages")
    .IsDependentOn("Pack")
    .Does(() =>
    {
        OctoPush(octopusServer, octopusApiKey, GetFiles("./package/*.nupkg"), new OctopusPushSettings
            {
                Space = octopusSpace
            });
    });

Task("CreateRelease")
    .IsDependentOn("PushPackages")
    .Does(() =>
    {
        OctoCreateRelease(octopusProject, new CreateReleaseSettings
            {
                Server = octopusServer,
                ApiKey = octopusApiKey,
                Space = octopusSpace,
                ReleaseNumber = packageVersion,
                DefaultPackageVersion = packageVersion
            });
    });

Task("DeployRelease")
    .IsDependentOn("CreateRelease")
    .Does(() =>
    {
        OctoDeployRelease(octopusServer, octopusApiKey, octopusProject, octopusEnvironment, packageVersion, new OctopusDeployReleaseDeploymentSettings
            {
                Space = octopusSpace,
                ShowProgress = true
            });
    });

Task("Default")
    .IsDependentOn("RunUnitTests");

RunTarget(target);
