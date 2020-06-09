using System;
using System.Linq;
using System.Reflection;
using DbUp;
using DbUp.SqlServer;
using DbUp.Engine;
using DbUp.Helpers;
using DbUp.Support;
using System.IO;

namespace OctopusSamples.OctoPetShopDatabase
{
    class Program
    {
        static int Main(string[] args)
        {
            var retryCount = 0;
            var environmentVariableConnectionString = Environment.GetEnvironmentVariable("DbUpConnectionString");
            var connectionString = environmentVariableConnectionString == null ? args.FirstOrDefault(x => x.StartsWith("--ConnectionString", StringComparison.OrdinalIgnoreCase)) : environmentVariableConnectionString;

            if (string.IsNullOrEmpty(environmentVariableConnectionString))
                connectionString = connectionString.Substring(connectionString.IndexOf("=") + 1).Replace(@"""", string.Empty);

            // retry three times
            while (true)
            {
                try
                {
                    EnsureDatabase.For.SqlDatabase(connectionString);
                    break;
                }
                catch (System.Data.SqlClient.SqlException)
                {
                    // check type
                    if (retryCount < 3)
                    {
                        // Display
                        Console.WriteLine("Connection error occured, waiting 3 seconds then trying again.");
                        System.Threading.Thread.Sleep(3000);
                        retryCount += 1;
                    }
                    else
                    {
                        // rethrow
                        throw;
                    }
                }
            }

            var upgrader =
                DeployChanges.To
                   .SqlDatabase(connectionString)
                   .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                   .LogToConsole()
                   .Build();

            if (args.Any(a => a.StartsWith("--PreviewReportPath", StringComparison.InvariantCultureIgnoreCase)))
            {
                // Generate a preview file so Octopus Deploy can generate an artifact for approvals
                var report = args.FirstOrDefault(x => x.StartsWith("--PreviewReportPath", StringComparison.OrdinalIgnoreCase));
                report = report.Substring(report.IndexOf("=") + 1).Replace(@"""", string.Empty);

                var fullReportPath = Path.Combine(report, "UpgradeReport.html");

                Console.WriteLine($"Generating the report at {fullReportPath}");

                upgrader.GenerateUpgradeHtmlReport(fullReportPath);
            }
            else
            {
                var result = upgrader.PerformUpgrade();

                // Display the result
                if (result.Successful)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Success!");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(result.Error);
                    Console.WriteLine("Failed!");
                }
            }
            return 0;
        }
    }
}
