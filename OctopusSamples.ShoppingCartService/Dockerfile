FROM gcr.io/google-appengine/aspnetcore:2.1
ADD ./ /app
ENV ASPNETCORE_URLS=http://*:${PORT}
WORKDIR /app
ENTRYPOINT ["dotnet", "OctopusSamples.ShoppingCartService.dll"]
