# Codefresh pipeline

This folder contains Codefresh pipelines to mirror the ones for GitHub.

## [dotnet-core-local.yaml](dotnet-core-local.yaml)
This is the main pipeline that builds the 4 different dotnet microservices:

* OctoPetShop Database
* OctoPetShop Web
* OctoPetShop Product Service
* OctoPetShop Cart Service

### Pre-reqs
It requires 2 variables to be setup:
* OCTOPUS_URL: The URL of your Octopus Server
* OCTOPUS_API_KEY: An API key to authenticate to the Octopus Server with

It also requires an annotation to be added to the pipeline:
* BUILD_NUMBER: used to create an auto-incremented build number used to create the package version

Optionally, you can add a it trigger
