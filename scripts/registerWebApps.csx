#r "C:\Program Files\Octopus Deploy\Octopus\Octopus.Client.dll"

using Octopus.Client;
using Octopus.Client.Model;
using Octopus.Client.Model.Endpoints;

var octopusURL = "https://your.octopus.server";
var octopusAPIKey = "API-YOURAPIKEY";

var endpoint = new OctopusServerEndpoint(octopusURL, octopusAPIKey);
var repository = new OctopusRepository(endpoint);

var spaceName = "Demo";

var resourceGroupName = "octopetshop";
var accountId = "subscription-account-id";

var webApps = new[] { "product", "shoppingcart", "web" };
var webAppNames = new Dictionary<string, string>();
var webAppRoles = new Dictionary<string, string>();
var environments = new[] { "Development", "Test", "Production" };

webAppNames["product"] = "Product API";
webAppNames["shoppingcart"] = "Shopping Cart API";
webAppNames["web"] = "Main Web App";

webAppRoles["product"] = "octopetshop-product-api";
webAppRoles["shoppingcart"] = "octopetshop-shoppingcart-api";
webAppRoles["web"] = "octopetshop-web-app";

try
{
    // Get the space to work in
    var space = repository.Spaces.FindByName(spaceName);
    Console.WriteLine($"Using Space named {space.Name} with id {space.Id}");

    // Create space specific repository
    var repositoryForSpace = repository.ForSpace(space);

    foreach (var environmentName in environments)
    {

        var environment = repositoryForSpace.Environments.FindByName(environmentName);
        var environmentLabel = environmentName.ToLower();

        foreach (var webApp in webApps)
        {
            var webAppName = webAppNames[webApp];
            var webAppRole = webAppRoles[webApp];
            var webAppResource = new MachineResource();

            webAppResource.Endpoint = new AzureWebAppEndpointResource
            {
                AccountId = accountId,
                ResourceGroupName = resourceGroupName,
                WebAppName = $"ops-{webApp}-{environmentLabel}"
            };

            webAppResource.Name = $"OctoPetShop {webAppName} - {environmentName}";
            webAppResource.AddOrUpdateEnvironments(environment);
            webAppResource.AddOrUpdateRoles(webAppRole);

            Console.WriteLine($"Creating web app target '{webAppResource.Name}'");
            repositoryForSpace.Machines.Create(webAppResource);
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
