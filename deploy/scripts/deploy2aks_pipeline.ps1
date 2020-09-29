param([string] $sourcesDirectory, [string] $resourceGroup, [string] $aksClusterName, [string] $redisCacheName, [string] $batchAccountName, [string] $storageAccountName, [string] $batchPoolName)

# Connect to Azure Kubernetes cluster 
Import-AzAksCredential -Force -ResourceGroupName $resourceGroup -Name $aksClusterName

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
    "AzureBatchPoolName": $batchPoolName,
    "SoaStorageConnectionString": $storageAccountConnectionString
}
"@

Out-File -InputObject $sessionJson -FilePath "./session.json"

# Create k8s configmap and secret
kubectl create configmap redis-config --from-literal redisCacheName=$redisCacheName --from-literal redisConnectionString=$redisConnectionString
kubectl create secret generic redis-secret --from-literal redisCacheAccessKey=$redisPrimaryKey
kubectl create secret generic session-secret --from-file=./session.json

kubectl apply -f "$sourcesDirectory/deploy/manifests/dispatcher.yaml"

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
        break
    }

    Start-Sleep -Seconds 5
}
$dispatcherIpAddressString = $dispatcherIpAddress.ToString()
kubectl create configmap dispatcher-config --from-literal dispatcherIpAddress=$dispatcherIpAddressString

kubectl apply -f "$sourcesDirectory/deploy/manifests/session.yaml"
kubectl apply -f "$sourcesDirectory/deploy/manifests/frontend.yaml"
