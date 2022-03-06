$domain = $args[0]

Write-Output( "Tagging Database Image" )
docker tag $domain/octopetshop-database:latest $domain/octopetshop-database:latest
Write-Output( "Pushing Database Image" )
docker push $domain/octopetshop-database:latest

Write-Output( "Tagging Product Service Image" )
docker tag $domain/octopetshop-productservice:latest $domain/octopetshop-productservice:latest
Write-Output( "Pushing Product Service Image" )
docker push $domain/octopetshop-productservice:latest

Write-Output( "Tagging Shopping Cart Service Image" )
docker tag $domain/octopetshop-shoppingcartservice:latest $domain/octopetshop-shoppingcartservice:latest
Write-Output( "Pushing Shopping Cart Service Image" )
docker push $domain/octopetshop-shoppingcartservice:latest

Write-Output( "Tagging Web Image" )
docker tag $domain/octopetshop-web:latest $domain/octopetshop-web:latest
Write-Output( "Pushing Web Image" )
docker push $domain/octopetshop-web:latest