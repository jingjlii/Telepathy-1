﻿Set-Location -Path C:\Users\jingjli\github\Telepathy\future\sparrow-poc
Start-Process cmd.exe -ArgumentList "/K dotnet run -p BrokerServer 50051"
Start-Process cmd.exe -ArgumentList "/K dotnet run -p BrokerServer 50052" 
Start-Process cmd.exe -ArgumentList "/K dotnet run -p BrokerServer 50053" 
Start-Process cmd.exe -ArgumentList "/K dotnet run -p WorkerServer 50054" 
Start-Process cmd.exe -ArgumentList "/K dotnet run -p WorkerServer 50055" 
Start-Process cmd.exe -ArgumentList "/K dotnet run -p WorkerServer 50056" 
Start-Process cmd.exe -ArgumentList "/K dotnet run -p WorkerServer 50057" 
Start-Process cmd.exe -ArgumentList "/K dotnet run -p WorkerServer 50058" 

Start-Sleep -s 5

$max_iterations = 30;

for ($i=1; $i -le $max_iterations; $i++)
{
	
	
	$proc = Start-Process cmd.exe -ArgumentList "/K dotnet run -p BrokerClient & exit" -PassThru
	$proc.WaitForExit()
  
}