# OctoPetShop
Octopus Pet Shop Example Web app written in .NET Core.  This solution consists of:
 - Octopus Pet Shop Web front end
 - Product Service
 - Shopping Cart Service
 - Database project using Dbup

 Included in this example are docker files to make each component as a container.  The docker-compose file at the root of the project also includes a SQL Server image so the entire application can be run in containers.  

 Kubernetes .yaml files have been included in this project which will pull the images from the octopussamples Docker Hub repo.

# Important Notes
- Password for SQL Server will need to be changed, current password will fail due to password requirements.  You will also need to update the password in octopetshop-sql-deployment.yaml, octopetshop-database-job.yaml, octopetshop-productservice-deployment.yaml, and octopetshop-shoppingcartservice-deployment.yaml.
- SQL Server image has not been configured with a persistent volume claim, you will lose your data if you re-create your Kubernetes cluster.
- When using Octopus Deploy to deploy the .yaml files to a Kubernetes cluster, be sure that the octopetshop-database-job.yaml is run AFTER both octopetshop-sqlserver-cluster-ip-service.yaml and octopetshop-sql-deployment.yaml.
