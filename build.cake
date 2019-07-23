#tool "nuget:?package=OctopusTools"

using Cake.Common.Tools.OctopusDeploy;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var version = Argument("packageVersion", "0.0.1");
var prerelease = Argument("prerelease", "");
var databaseRuntime = Argument("databaseRuntime", "win-x64");
var octopusServer = Argument("octopusServer", "https://your.octopus.server");
var octopusApiKey = Argument("octopusApiKey", "hey, don't commit your API key");

var packageVersion = $"{version}{prerelease}";

var projects = GetFiles("./**/*.csproj");

Task("Clean")
    .Does(() =>
        {
            CleanDirectory("publish");
            CleanDirectory("package");

            foreach(var project in projects) {
                DotNetCoreClean(project.GetDirectory().FullPath,
                    new DotNetCoreCleanSettings
                    {
                        Configuration = configuration
                    });
            }
        });

// Run dotnet restore to restore all package references.
Task("Restore")
    .Does(() =>
    {
        foreach(var project in projects)
        {
            var projectName = project.GetFilenameWithoutExtension().ToString();
            var restoreSettings = new DotNetCoreRestoreSettings();

            if (projectName == "OctopusSamples.OctoPetShop.Database")
            {
                restoreSettings.Runtime = databaseRuntime;
            }

            DotNetCoreRestore(project.GetDirectory().FullPath, restoreSettings);
        }
    });

 Task("Build")
    .Does(() =>
    {
        foreach(var project in projects)
        {
            var projectName = project.GetFilenameWithoutExtension().ToString();

            var buildSettings = new DotNetCoreBuildSettings()
                {
                    Configuration = configuration,
                    NoRestore = true
                };

            if (projectName == "OctopusSamples.OctoPetShop.Database")
            {
                buildSettings.Runtime = databaseRuntime;
            }

            DotNetCoreBuild(project.GetDirectory().FullPath, buildSettings);
        }
    });

Task("Publish")
    .Does(() =>
    {
        foreach(var project in projects)
        {
            var projectName = project.GetFilenameWithoutExtension().ToString();

            var publishSettings = new DotNetCorePublishSettings()
                {
                    Configuration = configuration,
                    OutputDirectory = System.IO.Path.Combine("publish", projectName),
                    ArgumentCustomization = args => args.Append("--no-restore")
                };

            if (projectName == "OctopusSamples.OctoPetShop.Database")
            {
                publishSettings.Runtime = databaseRuntime;
            }

            DotNetCorePublish(project.GetDirectory().FullPath, publishSettings);
        }

        // publish infrastructure
        CopyDirectory("OctopusSamples.OctoPetShop.Infrastructure", System.IO.Path.Combine("publish", "OctopusSamples.OctoPetShop.Infrastructure"));
    });

Task("Pack")
    .Does(() =>
    {
        foreach(var project in projects)
        {
            var projectName = project.GetFilenameWithoutExtension().ToString();

            OctoPack(
                projectName,
                new OctopusPackSettings()
                {
                    Format = OctopusPackFormat.NuPkg,
                    BasePath = System.IO.Path.Combine("publish", projectName),
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

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
    .IsDependentOn("Publish")
    .IsDependentOn("Pack");

RunTarget(target);
