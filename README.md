# OctoPetShop
Octopus Pet Shop Example Web app written in .NET Core.  This solution consists of:
 - Octopus Pet Shop Web front end
 - Product Service
 - Shopping Cart Service
 - Database project using Dbup

 Included in this example are docker files to make each component as a container.  The docker-compose file at the root of the project also includes a SQL Server image so the entire application can be run in containers.  

 Kubernetes .yaml files have been included in this project which will pull the images from the octopussamples Docker Hub repo.  Note: the Sql Server password is in plain text, these would usually be created as [Secrets](https://kubernetes.io/docs/concepts/configuration/secret/).  NOTE: SQL Server image has not been configured with a persistent volume claim, you will lose your data if you re-create your Kubernetes cluster.
