FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /app

# copy csproj and restore as distinct layers
#COPY *.csproj ./shoppingcartservice/
#RUN dotnet restore

# copy everything else and build app
COPY . ./shoppingcartservice/
WORKDIR /app/shoppingcartservice
RUN dotnet publish -c Release -o out

FROM microsoft/dotnet:2.1-aspnetcore-runtime AS runtime
WORKDIR /app
COPY --from=build /app/shoppingcartservice/out ./
ENTRYPOINT ["dotnet", "OctopusSamples.ShoppingCartService.dll"]
