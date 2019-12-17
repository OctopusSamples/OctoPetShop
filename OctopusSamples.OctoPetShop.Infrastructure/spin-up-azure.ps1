param (
    [switch] $tearDown  # SUPPLY THIS TO DROP ENTIRE RESOURCE GROUP
)

# === Stop... ===

# Respond "iam"  to confirm you want to proceed

if ($tearDown.IsPresent) {
    $response = Read-Host "*** TEAR DOWN *** AZURE INFRA - you sure?" 
} else {
    $response = Read-Host "SPIN UP AZURE INFRA - you sure?"
}

if ($response -ne "iam") {
    Write-Host "This isn't the script you're looking for..."
    exit
}

# === Carry on... ===

$subscriptionId = "xxxxxxx-xxxx-xxxx-xxxx-xxxxxxxx"         # Azure Subscription id
$resourceGroupName = "OctoPetShopRG"                        # Resource Group to create to contain all resources
$location = "uksouth"                                       # Azure geo-location
$environments = @('development','Test', 'Production')       # Your Octopus Environments
$envDBNames = @('dev','test', 'prod')                       # Your Octopus db env name (append to db name, so that using scoped variables makes sense!)

$webappname = "OctoPetShop-web-"                            # name of Azure web web-app - environment name will be appended to it
$apiappname = "OctoPetShop-api-"                            # name of Azure api web-app - environment name will be appended to it
$appServiceName = "octo-petshop"                            # name of Azure appservice
$ipWhiteList = @("<YOUR IP ADDRESS>", "<IP 2> (optional)")  # lock down access to VM by restricting access only to these IP addresses, comma separated

$sqlServerPassword = "<ADD PASSWORD>"                       # Strong password for the SQL Server
$sqlServerUser = "OctoPetShopUser"                          # SQL Server username
$sqlServername = "octopetshopsqlserver"                     # SQL Server instance name
$sqlSKU = "Basic"                                           # SQL Database sku

$vmName = "<SET VM NAME>"                                   # Azure Virtual Machine name - to be used as SQL Jump Box by Octopus
$vmDNSName = "<SET VM DNS NAME>"                            # Azure Virtual Machine DNS name - must be unique 
$vmPassword = "<ADD PASSWORD>"                              # Azure Virtual Machine strong password
$vmUsername = "OctoPetShopUser"                             # Azure Virtual Machine user name
$vmSize = "Standard_B1ms"                                   # Azure Virtual Machine size
$vmImage = "MicrosoftWindowsServer:WindowsServer:2019-Datacenter:latest"  # Valid URN format: "Publisher:Offer:Sku:Version".  value from: az vm image list, az vm image show

#=========================================================================================================

function Add-ResourceGroup () {
    Write-Host "Creating resource group - $($script:resourceGroupName)." -ForegroundColor Blue
    az group create -l  $script:location -n $script:resourceGroupName --subscription $script:subscriptionId
    Write-Host "Created resource group" -ForegroundColor Green
}

function Remove-ResourceGroup () {
    Write-Host "Removing resource group - $($script:resourceGroupName)" -ForegroundColor Blue
    az group delete -n $script:resourceGroupName --subscription $script:subscriptionId --yes
    Write-Host "Removed resource group" -ForegroundColor Green
}

function Add-IpRestrictions($appName) {

    $ipCount = 1

    foreach ($ip in $script:ipWhiteList) {   
        $ipAddress = "$($ip)/1"
        $ruleName = "ipRule$($ipCount.ToString())"
        
        Write-Host "Add IP restriction for ip: $ip" -ForegroundColor DarkBlue
        az webapp config access-restriction add --rule-name $ruleName --action Allow --ip-address $ipAddress --priority 100 -g $script:resourceGroupName -n $appName --subscription $script:subscriptionId
        $ipCount++
    }
}

function Add-WebApps () {
    Write-Host "Creating app service plan" -ForegroundColor Blue
    az appservice plan create --name $script:appServiceName --subscription $script:subscriptionId --sku "B1" --location $script:location --resource-group $script:resourceGroupName
    Write-Host "Created app service plan" -ForegroundColor Green

    foreach ($environment in $script:environments) {
        $webappfullname = $script:webappname + $environment
        $apiappfullname = $script:apiappname + $environment

        Write-Host "Creating $($environment) Web Apps" -ForegroundColor Blue
        az webapp create --name $webappfullname --resource-group $script:resourceGroupName --subscription $script:subscriptionId --plan $script:appServiceName
        az webapp create --name $apiappfullname --resource-group $script:resourceGroupName --subscription $script:subscriptionId --plan $script:appServiceName
        Write-Host "Created web apps" -ForegroundColor Green
    }
}

function Add-SqlServer() {
    Write-Host "Creating SQL Server" -ForegroundColor Blue
    az sql server create --admin-password $script:sqlServerPassword --admin-user $script:sqlServerUser --name $script:sqlServername --resource-group $script:resourceGroupName --location $script:location --subscription $script:subscriptionId 
    Write-Host "Created SQL Server" -ForegroundColor Green
}

function Add-Databases () {

    foreach ($environment in $script:envDBNames) {
        $sqldbname = "octopetshop-" + $environment
        Write-Host "Creating $($environment) database" -ForegroundColor Blue
        az sql db create --resource-group $script:resourceGroupName --subscription $script:subscriptionId --server $script:sqlServername --name $sqldbname --edition $script:sqlSKU
        Write-Host "Created database" -ForegroundColor Green
    }
}

function Add-JumpVM (){
    Write-Host "Creating jump box VM" -ForegroundColor Blue
    az vm create --admin-password $script:vmPassword --admin-user $script:vmUsername --name $script:vmName --resource-group $script:resourceGroupName --location $script:location --subscription $script:subscriptionId --size $script:vmSize --image $script:vmImage --public-ip-address-dns-name $script:vmDNSName
    Write-Host "Created  jump box VM" -ForegroundColor Green

    Write-Host "Restricting RDP to Jump Box" -ForegroundColor Blue

    $nsgName = "$($script:vmName)NSG"
    $priority = 100

    # Remove existing RDP rule
    az network nsg rule delete -n RDP --nsg-name $nsgName --resource-group $script:resourceGroupName --subscription $script:subscriptionId

    foreach ($ip in $script:ipWhiteList) {   
        $rdpRuleName = "RDP$($priority.ToString())"

        Write-Host "Add network rule for ip: $ip" -ForegroundColor DarkBlue
        az network nsg rule create -n $rdpRuleName --nsg-name $nsgName --resource-group $script:resourceGroupName --subscription $script:subscriptionId --priority $priority --access Allow --destination-port-ranges * --direction Inbound --protocol TCP --source-address-prefixes $ip --source-port-ranges *
        $priority++
    }

    Write-Host "Restricted RDP Access to Jump Box"
}

function Add-SqlFirewallRule() {
    Write-Host "Creating SQL Server Firewall Rules" -ForegroundColor Blue
    $jumpBoxIp = az vm show -d --resource-group $script:resourceGroupName --subscription $script:subscriptionId  -n $script:vmName --query publicIps -o tsv
    # Allow jump box to access SQL Server
    az sql server firewall-rule create -g $script:resourceGroupName --server $script:sqlServername --name jumpboxrulename --start-ip-address $jumpBoxIp --end-ip-address $jumpBoxIp --subscription $script:subscriptionId

    # Allow azure resources to access SQL Server
    az sql server firewall-rule create -g $script:resourceGroupName -s $script:sqlServername -n "accessFromAzureServices" --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0 --subscription $script:subscriptionId
    Write-Host "Created SQL Server Firewall Rules" -ForegroundColor Green
}

#=========================================================================================================

if ($tearDown.IsPresent) {
    Remove-ResourceGroup

    Write-Host "Tear down Complete" -ForegroundColor Green
} else {
    Add-ResourceGroup
    Add-WebApps
    Add-SqlServer
    Add-Databases
    Add-JumpVM

    Add-SqlFirewallRule

    Write-Host "Spin-up Complete" -ForegroundColor Green
}


