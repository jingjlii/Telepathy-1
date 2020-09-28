param([string] $resourceGroup, [string] $aksClusterName, [string] $redisCacheName, [string] $batchAccountName, [string] $storageAccountName)

# Connect to Azure Kubernetes cluster and install kubectl 
Import-AzAksCredential -Force -ResourceGroupName $resourceGroup -Name $aksClusterName
# Install-AzAksKubectl -Force
# chmod 777 "/root/.azure-kubectl/kubectl"
# if ($env:PATH) {
#     $env:PATH += ":/root/.azure-kubectl/"
# } else {
#     $env:PATH = "/root/.azure-kubectl/"
# }
# curl "https://codeload.github.com/HyphonGuo/Telepathy/tar.gz/dev-integration" --output "archive.tar.gz"
# tar -xzf "archive.tar.gz"

$redisAccessKeys = Get-AzRedisCacheKey -ResourceGroupName $resourceGroup -Name $redisCacheName
$redisPrimaryKey = $redisAccessKeys.PrimaryKey
kubectl create configmap redis-config --from-literal redisCacheName=$redisCacheName
kubectl create secret generic redis-secret --from-literal redisCacheAccessKey=$redisPrimaryKey

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
