param([string] $telepathyRepoUrl, [string] $resourceGroup, [string] $aksClusterName, [string] $redisCacheName, [string] $batchAccountName, [string] $storageAccountName, [string] $batchAccountPoolName)

# Connect to Azure Kubernetes cluster and install kubectl 
Import-AzAksCredential -Force -ResourceGroupName $resourceGroup -Name $aksClusterName
Install-AzAksKubectl -Force
chmod 777 "/root/.azure-kubectl/kubectl"
if ($env:PATH) {
    $env:PATH += ":/root/.azure-kubectl/"
} else {
    $env:PATH = "/root/.azure-kubectl/"
}
curl  --output "archive.tar.gz"
tar -xzf "archive.tar.gz"

# Fetch information of required resources from Azure
$redisAccessKeys = Get-AzRedisCacheKey -ResourceGroupName $resourceGroup -Name $redisCacheName
$redisPrimaryKey = $redisAccessKeys.PrimaryKey
$redisConnectionString = "$redisCacheName.redis.cache.windows.net:6380,password=$redisPrimaryKey,ssl=True,abortConnect=False"

$batchAccountContext = Get-AzBatchAccount -ResourceGroupName $resourceGroup -Name $batchAccountName
$batchAccountKeys = Get-AzBatchAccountKeys -ResourceGroupName $resourceGroup -Name $batchAccountName
$storageAccountKeys = Get-AzStorageAccountKey -ResourceGroupName $resourceGroup -Name $storageAccountName
$batchAccountKey = $batchAccountKeys.PrimaryAccountKey
$storageAccountKey = $storageAccountKeys.Value[0]
$batchAccountEndpoint = $batchAccountContext.AccountEndpoint
$storageAccountConnectionString = "DefaultEndpointsProtocol=https;AccountName=$storageAccountName;AccountKey=$storageAccountKey;EndpointSuffix=core.windows.net"

$sessionJson = @"
{
    "RedisConnectionKey": $redisConnectionString,
    "AzureBatchServiceUrl": $batchAccountEndpoint,
    "AzureBatchAccountName": $batchAccountName,
    "AzureBatchAccountKey": $batchAccountKey,
    "AzureBatchPoolName": $batchAccountPoolName,
    "SoaStorageConnectionString": $storageAccountConnectionString
}
"@

Out-File -InputObject $sessionJson -FilePath "./session.json"

# Create k8s configmap and secret
kubectl create configmap redis-config --from-literal redisCacheName=$redisCacheName
kubectl create secret generic redis-secret --from-literal redisCacheAccessKey=$redisPrimaryKey --from-literal redisConnectionString=$redisConnectionString
kubectl create secret generic session-secret --from-file=./session.json

kubectl apply -f "Telepathy-dev-integration/deploy/manifests/dispatcher.yaml"

$dispatcherIpAddress = [System.Net.IPAddress]::None

while ($true) {
    $serviceInfo = kubectl get service
    $isValidIpAddress = $false
    foreach ($service in $serviceInfo) {
        $serviceProps = $service.Split(' ', [System.StringSplitOptions]::RemoveEmptyEntries)
        $serviceName = $serviceProps[0]
        $serviceExternalIp = $serviceProps[3]
        if ([string]::Equals($serviceName, "dispatcher")) {
            $isValidIpAddress = [System.Net.IPAddress]::TryParse($serviceExternalIp, [ref] $dispatcherIpAddress)
        }
    }

    if ($isValidIpAddress) {
        break;
    }

    Start-Sleep -Seconds 5
}
$dispatcherIpAddressString = $dispatcherIpAddress.ToString()
kubectl create configmap dispatcher-config --from-literal dispatcherIpAddress=$dispatcherIpAddressString

kubectl apply -f "Telepathy-dev-integration/deploy/manifests/session.yaml"
kubectl apply -f "Telepathy-dev-integration/deploy/manifests/frontend.yaml"
