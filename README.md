# OctoPetShop
Octopus Pet Shop Example Web app written in .NET Core.  This solution consists of:
 - Octopus Pet Shop Web front end
 - Product Service
 - Shopping Cart Service
 - Database project using Dbup

 Included in this example are docker files to make each component as a container.  The docker-compose file at the root of the project also includes a SQL Server image so the entire application can be run in containers.  NOTE: to run the application using docker-compose, be sure to update the service Urls in the OctopusSamples.OctoPetShop.Web appsettings.json to the IP addresses in the docker-compose file as well as update the database connection strings for both OctopusSamples.OctoPetShop.ShoppingCartService and OctopusSamples.OctoPetShop.ProductService projects.
