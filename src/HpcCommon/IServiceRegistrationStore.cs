﻿namespace Microsoft.Hpc
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IServiceRegistrationStore
    {
        Task<string> GetMd5Async(string serviceName, Version serviceVersion);

        Task<string> GetAsync(string serviceName, Version serviceVersion);

        Task SetAsync(string serviceName, Version serviceVersion, string serviceRegistration);

        Task DeleteAsync(string serviceName, Version serviceVersion);

        /// <summary>
        /// Enumerate serivce registration files
        /// </summary>
        /// <returns>Service registration file name list</returns>
        Task<List<string>> EnumerateAsync();

        /// <summary>
        /// Import HA service registration file from specified file.
        /// </summary>
        /// <param name="filePath">Path of service registration file which will be imported.</param>
        /// <param name="serviceName">Service name of what will be imported. If is <see langword="null"></see>, service name will be inferred from file name.</param>
        /// <returns>Async promise.</returns>
        Task ImportFromFileAsync(string filePath, string serviceName);

        Task<string> ExportToTempFileAsync(string serviceName, Version serviceVersion);
    }
}