﻿namespace Microsoft.Hpc
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IFabricContext
    {
        Task<string> ResolveSingletonServicePrimaryAsync(string serviceName, CancellationToken token);

        Task<IEnumerable<string>> ResolveStatelessServiceNodesAsync(string serviceName, CancellationToken token);

        Task<IEnumerable<string>> GetNodesAsync(CancellationToken token);

        IRegistry Registry { get; }

        EndpointsConnectionString ConnectionString { get; }

        HpcContextOwner Owner { get; }
    }
}