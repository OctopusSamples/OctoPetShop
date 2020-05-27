using System;
using System.Linq;
using System.Reflection;
using DbUp;
using DbUp.SqlServer;

namespace OctopusSamples.OctoPetShopDatabase
{
    class Program
    {
        static int Main(string[] args)
        {
            var retryCount = 0;
            var environmentVariableConnectionString = Environment.GetEnvironmentVariable("DbUpConnectionString");
            var connectionString = environmentVariableConnectionString == null ? args.FirstOrDefault() ?? "Server=(local)\\SqlExpress; Database=ops; Trusted_connection=true" : environmentVariableConnectionString;


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

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Error);
                Console.ResetColor();
#if DEBUG
                Console.ReadLine();
#endif
                return -1;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Success!");
            Console.ResetColor();

#if DEBUG
            Console.ReadLine();
#endif

            return 0;
        }
    }
}
