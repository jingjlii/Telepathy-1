param([string] $resourceGroup, [string] $aksClusterName)

Import-AzAksCredential -Force -ResourceGroupName $resourceGroup -Name $aksClusterName

Install-AzAksKubectl -Force

chmod 777 "/root/.azure-kubectl/kubectl"

if ($env:PATH) {
    $env:PATH += ":/root/.azure-kubectl/"
} else {
    $env:PATH = "/root/.azure-kubectl/"
}

curl "https://codeload.github.com/HyphonGuo/Telepathy/tar.gz/dev-integration" --output "archive.tar.gz"
tar -xzf "archive.tar.gz"

kubectl apply -f "Telepathy-dev-integration/deploy/manifests/"

# kubectl apply -f "https://raw.githubusercontent.com/HyphonGuo/pipelines-javascript-docker/master/manifests/deployment.yml"
# kubectl apply -f "https://raw.githubusercontent.com/HyphonGuo/pipelines-javascript-docker/master/manifests/service.yml"

