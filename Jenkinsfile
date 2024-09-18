pipeline {
    agent any
    stages {
            stage ('Set version') {
                steps {
                    script {
                        TEST_VAR = new Date().format("yyyy.MM.dd") as String
                        echo "${TEST_VAR}"
                        VERSION_NUMBER = VersionNumber(versionNumberString: "${TEST_VAR}.${BUILD_ID}")
                        currentBuild.displayName = "${VERSION_NUMBER}"
                    }
                }
            }
            stage ('Restore NuGet packages') {
                steps {
                    bat "dotnet restore \"${workspace}/OctopusSamples.OctoPetShop.Database/OctopusSamples.OctoPetShop.Database.csproj\""
                    bat "dotnet restore \"${workspace}/OctopusSamples.OctoPetShop.ProductService/OctopusSamples.OctoPetShop.Productservice.csproj\""
                    bat "dotnet restore \"${workspace}/OctopusSamples.OctoPetShop.ShoppingCartService/OctopusSamples.OctoPetShop.ShoppingCartService.csproj\""
                    bat "dotnet restore \"${workspace}/OctopusSamples.OctoPetShop.Web/OctopusSamples.OctoPetShop.Web.csproj\""
                }
            }
            stage ('Build') {
                steps {
                    bat "dotnet build \"${workspace}/OctopusSamples.OctoPetShop.Database/OctopusSamples.OctoPetShop.Database.csproj\" --output \"${workspace}/output/OctopusSamples.OctoPetShop.Database \""
                    bat "dotnet publish \"${workspace}/OctopusSamples.OctoPetShop.ProductService/OctopusSamples.OctoPetShop.Productservice.csproj\" --output \"${workspace}/output/OctopusSamples.OctoPetShop.ProductService \""
                    bat "dotnet publish \"${workspace}/OctopusSamples.OctoPetShop.ShoppingCartService/OctopusSamples.OctoPetShop.ShoppingCartService.csproj\" --output \"${workspace}/output/OctopusSamples.OctoPetShop.ShoppingCartService \""
                    bat "dotnet publish \"${workspace}/OctopusSamples.OctoPetShop.Web/OctopusSamples.OctoPetShop.Web.csproj\" --output \"${workspace}/output/OctopusSamples.OctoPetShop.Web \""                    
                }
            }
            stage ('Zip') {
                steps {
                    zip archive: true, dir: "${workspace}/output/OctopusSamples.OctoPetShop.Database", glob: "", zipFile: "OctopusSamples.OctoPetShop.Database.${VERSION_NUMBER}.zip"
                    zip archive: true, dir: "${workspace}/output/OctopusSamples.OctoPetShop.ProductService", glob: "", zipFile: "OctopusSamples.OctoPetShop.ProductService.${VERSION_NUMBER}.zip"
                    zip archive: true, dir: "${workspace}/output/OctopusSamples.OctoPetShop.ShoppingCartService", glob: "", zipFile: "OctopusSamples.OctoPetShop.ShoppingCartService.${VERSION_NUMBER}.zip"
                    zip archive: true, dir: "${workspace}/output/OctopusSamples.OctoPetShop.Web", glob: "", zipFile: "OctopusSamples.OctoPetShop.Web.${VERSION_NUMBER}.zip"
                }
            }
            stage ('Push') {
                steps {
                    octopusPushPackage \
                        overwriteMode: "FailIfExists", \
                        packagePaths: "${workspace}/OctopusSamples.OctoPetShop.Database.${VERSION_NUMBER}.zip", \
                        serverId: "Octopus Deploy", \
                        spaceId: "Spaces-1", \
                        toolId: "Default"

                    octopusPushPackage \
                        overwriteMode: "FailIfExists", \
                        packagePaths: "${workspace}/OctopusSamples.OctoPetShop.Web.${VERSION_NUMBER}.zip", \
                        serverId: "Octopus Deploy", \
                        spaceId: "Spaces-1", \
                        toolId: "Default"

                    octopusPushPackage \
                        overwriteMode: "FailIfExists", \
                        packagePaths: "${workspace}/OctopusSamples.OctoPetShop.ProductService.${VERSION_NUMBER}.zip", \
                        serverId: "Octopus Deploy", \
                        spaceId: "Spaces-1", \
                        toolId: "Default"
                    
                    octopusPushPackage \
                        overwriteMode: "FailIfExists", \
                        packagePaths: "${workspace}/OctopusSamples.OctoPetShop.ShoppingCartService.${VERSION_NUMBER}.zip", \
                        serverId: "Octopus Deploy", \
                        spaceId: "Spaces-1", \
                        toolId: "Default"                }
            }
    }
}
