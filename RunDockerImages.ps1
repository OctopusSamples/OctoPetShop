$domain = $args[0]

try {
    docker network rm octonet
}
catch {
    Write-Output( "Network doesn't exist... Creating 'octonet' network." )
}

Write-Output( "Creating Network" )

docker network create octonet

Write-Output( "Wait for 2 seconds" )

Start-Sleep -Seconds 2

#docker run -d -p 1433:1433 -e SA_PASSWORD=OctopusDeploy_1234 -e ACCEPT_EULA=Y --name sql-server mcr.microsoft.com/mssql/server:2019-latest --network octonet

Write-Output( "Starting SQL Server Container" )

docker run -d -p 1433:1433 -e SA_PASSWORD=OctopusDeploy_1234 -e ACCEPT_EULA=Y --name sql-server mcr.microsoft.com/mssql/server:2019-latest
docker network connect octonet sql-server

Write-Output( "Starting Database Container" )

docker run -d -e DbUpConnectionString="Data Source=sql-server;Initial Catalog=OctoPetShop; User ID=sa; Password=OctopusDeploy_1234" --name database $domain/octopetshop-database
docker network connect octonet database

Write-Output( "Starting Shopping Cart Service Container" )

docker run -d -p 5012:80 -p 5013:443 -e OPSConnectionString="Data Source=sql-server;Initial Catalog=OctoPetShop; User ID=sa; Password=OctopusDeploy_1234" --name shoppingcartservice $domain/octopetshop-shoppingcartservice
docker network connect octonet shoppingcartservice

Write-Output( "Starting Product Service Container" )

docker run -d -p 5014:80 -p 5015:443 -e OPSConnectionString="Data Source=sql-server;Initial Catalog=OctoPetShop; User ID=sa; Password=OctopusDeploy_1234" --name productservice $domain/octopetshop-productservice
docker network connect octonet productservice

Write-Output( "Starting Web Container" )

docker run -d -p 5000:80 -p 5001:443 -e ProductServiceBaseUrl="http://productservice" -e ShoppingCartServiceBaseUrl="http://shoppingcartservice" --name web $domain/octopetshop-web
docker network connect octonet web

Write-Output( "All Services Up and Running, you can visit the OctoPetShop site at http://localhost:5000" )
