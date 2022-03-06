$domain = $args[0]

Write-Output( "Building Database Image" )
docker build -f OctopusSamples.OctoPetShop.Database\Dockerfile --force-rm -t $domain/octopetshop-database .

Write-Output( "Building Product Service Image" )
docker build -f OctopusSamples.OctoPetShop.ProductService\Dockerfile --force-rm -t $domain/octopetshop-productservice .

Write-Output( "Building Shopping Cart Service Image" )
docker build -f OctopusSamples.OctoPetShop.ShoppingCartService\Dockerfile --force-rm -t $domain/octopetshop-shoppingcartservice .

Write-Output( "Building Web Image" )
docker build -f OctopusSamples.OctoPetShop.Web\Dockerfile --force-rm -t $domain/octopetshop-web .
