﻿namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher
{
    using CommandLine;

    internal class SessionLauncherStartOption
    {
        [Option('s', "AzureBatchServiceUrl", SetName = "AzureBatch")]
        public string AzureBatchServiceUrl { get; set; }

        [Option('n', "AzureBatchAccountName", SetName = "AzureBatch")]
        public string AzureBatchAccountName { get; set; }

        [Option('k', "AzureBatchAccountKey", SetName = "AzureBatch")]
        public string AzureBatchAccountKey { get; set; }

        [Option('j', "AzureBatchJobId", SetName = "AzureBatch")]
        public string AzureBatchJobId { get; set; }

        [Option('p', "AzureBatchPoolName", SetName = "AzureBatch")]
        public string AzureBatchPoolName { get; set; }

        [Option('c', "AzureBatchBrokerStorageConnectionString", SetName = "AzureBatch")]
        public string AzureBatchBrokerStorageConnectionString { get; set; }

        [Option('h', "HpcPackSchedulerAddress", SetName = "HpcPack")]
        public string HpcPackSchedulerAddress { get; set; }

        [Option("BrokerLauncherExePath")]
        public string BrokerLauncherExePath { get; set; }

        [Option("ServiceHostExePath", SetName = "Local")]
        public string ServiceHostExePath { get; set; }

        [Option("ServiceRegistrationPath", SetName = "Local")]
        public string ServiceRegistrationPath { get; set; }

        [Option("LocalBrokerStorageConnectionString", SetName = "Local")]
        public string LocalBrokerStorageConnectionString { get; set; }

        [Option("SessionLauncherStorageConnectionString")]
        public string SessionLauncherStorageConnectionString { get; set; }

        [Option('f', "JsonFilePath", SetName = "JsonFile")]
        public string JsonFilePath { get; set; }

        [Option('d', HelpText = "Start as console application")]
        public bool AsConsole { get; set; }
    }
}