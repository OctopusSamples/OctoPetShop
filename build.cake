#tool "nuget:?package=OctopusTools&version=6.13.1"

using Cake.Common.Tools.OctopusDeploy;

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var version = Argument("packageVersion", "0.0.1");
var prerelease = Argument("prerelease", "");
var octopusServer = Argument("octopusServer", "https://your.octopus.server");
var octopusApiKey = Argument("octopusApiKey", "hey, don't commit your API key");
var octopusSpace = Argument("octopusSpace", "Default");
var octopusProject = Argument("octopusProject", "OctoPetShop");
var octopusEnvironment = Argument("octopusEnvironment", "Development");
var packageVersion = "";

var database = "OctopusSamples.OctoPetShop.Database";
var databaseProject = $"./{database}/{database}.csproj";
var productService = "OctopusSamples.OctoPetShop.ProductService";
var productServiceProject = $"./{productService}/{productService}.csproj";
var productServiceTests = "OctopusSamples.OctoPetShop.ProductService.Tests";
var productServiceTestsProject = $"./{productServiceTests}/{productServiceTests}.csproj";
var web = "OctopusSamples.OctoPetShop.Web";
var webProject = $"./{web}/{web}.csproj";

Setup(context =>
{
    var isLocalBuild = BuildSystem.IsLocalBuild;

    if (isLocalBuild && string.IsNullOrEmpty(prerelease))
    {
        prerelease = "-local";
    }

    packageVersion = $"{version}{prerelease}";

    Information("Building OctoPetShop v{0}", packageVersion);
});

Task("Clean")
    .Does(() =>
        {
            CleanDirectory("publish");
            CleanDirectory("package");
            CleanDirectory("testResults");

            var cleanSettings = new DotNetCoreCleanSettings { Configuration = configuration };

            DotNetCoreClean(databaseProject, cleanSettings);
            DotNetCoreClean(productServiceProject, cleanSettings);
            DotNetCoreClean(productServiceTestsProject, cleanSettings);
            DotNetCoreClean(webProject, cleanSettings);
        });

// Run dotnet restore to restore all package references.
Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
    {
            var restoreSettings = new DotNetCoreRestoreSettings();

            DotNetCoreRestore(databaseProject, new DotNetCoreRestoreSettings { Runtime = "win-x64" });
            DotNetCoreRestore(productServiceProject, restoreSettings);
            DotNetCoreRestore(productServiceTestsProject, restoreSettings);
            DotNetCoreRestore(webProject, restoreSettings);
    });

 Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(() =>
    {
            var buildSettings = new DotNetCoreBuildSettings
                {
                    Configuration = configuration,
                    NoRestore = true
                };

            DotNetCoreBuild(databaseProject, new DotNetCoreBuildSettings
                {
                    Configuration = configuration,
                    NoRestore = true,
                    Runtime = "win-x64"
                });
            DotNetCoreBuild(productServiceProject, buildSettings);
            DotNetCoreBuild(productServiceTestsProject, buildSettings);
            DotNetCoreBuild(webProject, buildSettings);
    });

Task("RunUnitTests")
    .IsDependentOn("Build")
    .Does(() =>
    {
        var testSettings = new DotNetCoreTestSettings
            {
                Configuration = configuration,
                NoRestore = true,
                NoBuild = true,
                Logger = "TRX",
                ResultsDirectory = "testResults"
            };

        DotNetCoreTest(productServiceTestsProject, testSettings);
    });

Task("Publish")
    .IsDependentOn("RunUnitTests")
    .Does(() =>
    {
        DotNetCorePublish(databaseProject, new DotNetCorePublishSettings()
                {
                    Configuration = configuration,
                    OutputDirectory = System.IO.Path.Combine("publish", database),
                    ArgumentCustomization = args => args.Append("--no-restore"),
                    Runtime = "win-x64"
                });
        DotNetCorePublish(productServiceProject, new DotNetCorePublishSettings()
                {
                    Configuration = configuration,
                    OutputDirectory = System.IO.Path.Combine("publish", productService),
                    ArgumentCustomization = args => args.Append("--no-restore")
                });
        DotNetCorePublish(webProject, new DotNetCorePublishSettings()
                {
                    Configuration = configuration,
                    OutputDirectory = System.IO.Path.Combine("publish", web),
                    ArgumentCustomization = args => args.Append("--no-restore")
                });
    });

Task("Pack")
    .IsDependentOn("Publish")
    .Does(() =>
    {
        Console.WriteLine($"Calling OctoPack for {database}");
        OctoPack(
            database,
            new OctopusPackSettings()
            {
                Format = OctopusPackFormat.Zip,
                BasePath = System.IO.Path.Combine("publish", database),
                OutFolder = "package",
                Version = packageVersion
            });

        Console.WriteLine($"Calling OctoPack for {productService}");
        OctoPack(
            productService,
            new OctopusPackSettings()
            {
                Format = OctopusPackFormat.Zip,
                BasePath = System.IO.Path.Combine("publish", productService),
                OutFolder = "package",
                Version = packageVersion
            });

        Console.WriteLine($"Calling OctoPack for {web}");
        OctoPack(
            web,
            new OctopusPackSettings()
            {
                Format = OctopusPackFormat.Zip,
                BasePath = System.IO.Path.Combine("publish", web),
                OutFolder = "package",
                Version = packageVersion
            });
    });

Task("PushPackages")
    .IsDependentOn("Pack")
    .Does(() =>
    {
        OctoPush(octopusServer, octopusApiKey, GetFiles("./package/*.zip"), new OctopusPushSettings
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
