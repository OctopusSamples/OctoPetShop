chmod +x ./OctopusSamples.OctoPetShop.Database
connectionString=$(get_octopusvariable "ConnectionStrings:OPSConnectionString")
./OctopusSamples.OctoPetShop.Database "$connectionString"
