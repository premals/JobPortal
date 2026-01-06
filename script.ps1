param(
    [string]$Registry = "acrvkpshared.azurecr.io/jobportal",

    # Azure Cosmos DB (Mongo API)
    [string]$MongoConnectionString = "mongodb+srv://vedant:jobportal%401997@jobportal.global.mongocluster.cosmos.azure.com/?tls=true&authMechanism=SCRAM-SHA-256&retrywrites=false&maxIdleTimeMS=120000",
    [string]$MongoAuthDb = "AuthDb",
    [string]$MongoJobProviderDb = "JobProviderDb",
    [string]$MongoJobSeekerDb = "JobSeekerDb",

    # Azure Service Bus
    [string]$ServiceBusConnectionString = "Endpoint=sb://sb-vkp-shared.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=***",
    [string]$IdentityTopic = "identity-topic",
    [string]$JobApplicationsTopic = "jobapplications-topic",
    [string]$JobProviderTopic = "jobprovider-topic",
    [string]$JobProviderSubscription = "jobprovider-sub",
    [string]$JobEventsTopic = "jobs-topic",
    [string]$JobEventsSubscription = "jobseeker-sub",
    [string]$JobSeekerSubscription = "jobseeker-sub"
)

$ErrorActionPreference = "Stop"

# -------------------------------------------------
# Helper functions
# -------------------------------------------------
function Remove-ContainerIfExists {
    param([string]$Name)
    if (docker ps -a --format "{{.Names}}" | Where-Object { $_ -eq $Name }) {
        docker rm -f $Name | Out-Null
    }
}

function Build-BackendImage {
    param(
        [string]$ImageName,
        [string]$DockerfilePath
    )

    Write-Host "Building image: $ImageName"

    $fullTag = "${Registry}/${ImageName}:latest"

    Write-Host "Tag: $fullTag"
    Write-Host "Dockerfile: $DockerfilePath"
    Write-Host "Context: RecrutingAPP"

    docker build -f $DockerfilePath -t $fullTag RecrutingAPP

    if ($LASTEXITCODE -ne 0) {
        throw "Docker build failed for $ImageName"
    }
}

# -------------------------------------------------
# Validate required inputs
# -------------------------------------------------
if (-not $ServiceBusConnectionString) {
    throw "ServiceBusConnectionString is required."
}

# -------------------------------------------------
# Build BACKEND images (same context)
# -------------------------------------------------
Build-BackendImage -ImageName "gateway"             -DockerfilePath "RecrutingAPP/Gateway/Dockerfile"
Build-BackendImage -ImageName "identity-service"    -DockerfilePath "RecrutingAPP/IdendityService/IdendityService/Dockerfile"
Build-BackendImage -ImageName "jobprovider-service" -DockerfilePath "RecrutingAPP/JobProviderService/Dockerfile"
Build-BackendImage -ImageName "jobseeker-service"   -DockerfilePath "RecrutingAPP/JobSeekerService/Dockerfile"

# -------------------------------------------------
# Build FRONTEND image (different context)
# -------------------------------------------------
Write-Host "Building image: frontend"

$frontendTag = "${Registry}/frontend:latest"
$frontendDockerfile = "JobPortalUi/career-connect-ui/Dockerfile"
$frontendContext = "JobPortalUi/career-connect-ui"

Write-Host "Tag: $frontendTag"
Write-Host "Dockerfile: $frontendDockerfile"
Write-Host "Context: $frontendContext"

docker build -f $frontendDockerfile -t $frontendTag $frontendContext

if ($LASTEXITCODE -ne 0) {
    throw "Docker build failed for frontend"
}

Write-Host "All images built successfully"

# -------------------------------------------------
# Prepare Ocelot config
# -------------------------------------------------
$ocelotSource   = Join-Path $PSScriptRoot "RecrutingAPP\Gateway\ocelot.json"
$ocelotOverride = Join-Path $env:TEMP "ocelot.override.json"

(Get-Content $ocelotSource) `
    -replace '"Host": "localhost"', '"Host": "host.docker.internal"' |
    Set-Content $ocelotOverride

# -------------------------------------------------
# Remove old containers
# -------------------------------------------------
"jobportal-identity",
"jobportal-jobprovider",
"jobportal-jobseeker",
"jobportal-gateway",
"jobportal-frontend" | ForEach-Object {
    Remove-ContainerIfExists $_
}

# -------------------------------------------------
# Run containers (actual ports)
# -------------------------------------------------

Write-Host "Starting Identity Service"
docker run -d --name jobportal-identity `
    -p 5198:5198 `
    -e "ASPNETCORE_URLS=http://+:5198" `
    -e "MongoSettings__ConnectionString=$MongoConnectionString" `
    -e "MongoSettings__DatabaseName=$MongoAuthDb" `
    -e "Messaging__Provider=AzureServiceBus" `
    -e "Messaging__AzureServiceBus__ConnectionString=$ServiceBusConnectionString" `
    -e "Messaging__AzureServiceBus__IdentityTopic=$IdentityTopic" `
    "${Registry}/identity-service:latest" | Out-Null


Write-Host "Starting Job Provider Service"
docker run -d --name jobportal-jobprovider `
    -p 5133:5133 `
    -e "ASPNETCORE_URLS=http://+:5133" `
    -e "Mongo__ConnectionString=$MongoConnectionString" `
    -e "Mongo__Database=$MongoJobProviderDb" `
    -e "Messaging__Provider=AzureServiceBus" `
    -e "Messaging__AzureServiceBus__ConnectionString=$ServiceBusConnectionString" `
    -e "Messaging__AzureServiceBus__Topic=$JobProviderTopic" `
    -e "Messaging__AzureServiceBus__JobApplicationsTopic=$JobApplicationsTopic" `
    -e "Messaging__AzureServiceBus__JobProviderSubscription=$JobProviderSubscription" `
    "${Registry}/jobprovider-service:latest" | Out-Null


Write-Host "Starting Job Seeker Service"
docker run -d --name jobportal-jobseeker `
    -p 5025:5025 `
    -e "ASPNETCORE_URLS=http://+:5025" `
    -e "Mongo__ConnectionString=$MongoConnectionString" `
    -e "Mongo__DatabaseName=$MongoJobSeekerDb" `
    -e "Messaging__Provider=AzureServiceBus" `
    -e "Messaging__AzureServiceBus__ConnectionString=$ServiceBusConnectionString" `
    -e "Messaging__AzureServiceBus__JobApplicationsTopic=$JobApplicationsTopic" `
    -e "Messaging__AzureServiceBus__IdentityTopic=$IdentityTopic" `
    -e "Messaging__AzureServiceBus__JobEventsTopic=$JobEventsTopic" `
    -e "Messaging__AzureServiceBus__JobEventsSubscription=$JobEventsSubscription" `
    -e "Messaging__AzureServiceBus__JobSeekerSubscription=$JobSeekerSubscription" `
    "${Registry}/jobseeker-service:latest" | Out-Null


Write-Host "Starting Gateway"
docker run -d --name jobportal-gateway `
    -p 5000:5000 `
    -e "ASPNETCORE_URLS=http://+:5000" `
    -v "${ocelotOverride}:/app/ocelot.json" `
    "${Registry}/gateway:latest" | Out-Null


Write-Host "Starting Frontend"
docker run -d --name jobportal-frontend `
    -p 4000:4000 `
    "${Registry}/frontend:latest" | Out-Null


Write-Host ""
Write-Host "ALL CONTAINERS STARTED SUCCESSFULLY"
Write-Host "Frontend URL: http://localhost:4000"
Write-Host "Gateway URL: http://localhost:5000"
