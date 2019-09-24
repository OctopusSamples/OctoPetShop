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
            var environmentVariableConnectionString = Environment.GetEnvironmentVariable("DbUpConnectionString");
            var connectionString = environmentVariableConnectionString == null ? args.FirstOrDefault() ?? "Server=(local)\\SqlExpress; Database=ops; Trusted_connection=true" : environmentVariableConnectionString;

            EnsureDatabase.For.SqlDatabase(connectionString);

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
