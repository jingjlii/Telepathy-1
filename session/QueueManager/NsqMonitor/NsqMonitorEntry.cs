﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using StackExchange.Redis;

namespace Microsoft.Telepathy.QueueManager.NsqMonitor
{
    using Microsoft.Telepathy.Session;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Telepathy.Common;

    public class NsqMonitorEntry
    {
        public string SessionId { get; }

        private string BatchId { get; }

        public event EventHandler Exit;

        public BatchClientState CurrentQueueState { get; private set; }

        private object changeQueueStateLock = new object();

        private NsqMonitor _batchQueueMonitor;

        private int _clientTimeout;

        public string BatchQueueId { get; }

        private IDatabase _cache;

        public NsqMonitorEntry(string sessionId, string batchId, int clientTimeout)
        {
            SessionId = sessionId;
            BatchId = batchId;
            _clientTimeout = clientTimeout;
            BatchQueueId = SessionConfigurationManager.GetBatchClientQueueId(sessionId, batchId);
            _cache = RedisDB.Connection.GetDatabase();
        }

        public async Task StartAsync()
        {
            this.CurrentQueueState = BatchClientState.Active;
            _batchQueueMonitor = new NsqMonitor(SessionId, BatchId, _clientTimeout, this.BatchQueueMonitor_OnReportQueueState);
            try
            {
                Task.Run(() => this.StartMonitorAsync());
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private async Task StartMonitorAsync()
        {
            try
            {
                RetryManager retry = new RetryManager(new ExponentialBackoffRetryTimer(2000, 30000), 5);
                await RetryHelper<Task>.InvokeOperationAsync(
                    async () =>
                    {
                        await this._batchQueueMonitor.StartAsync();
                        return null;
                    },
                    async (e, r) => await Task.FromResult<object>(new Func<object>(() => { return null; }).Invoke()),
                    retry);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private async void BatchQueueMonitor_OnReportQueueState(string batchId, BatchClientState state, bool shouldExit)
        {
            Console.WriteLine($"[NsqMonitorEntry] {BatchQueueId} : Current queue state is {state}");
            if (CurrentQueueState != state)
            {
                lock (changeQueueStateLock)
                {
                    if (CurrentQueueState != state)
                    {
                        //Update stored batchclient state in Redis
                        var hashKey = SessionConfigurationManager.GetRedisBatchClientStateKey(SessionId);
                        var storedClientState = _cache.HashGet(hashKey, BatchId).ToString();
                        if (string.Equals(storedClientState, BatchClientState.EndOfRequest.ToString()) ||
                            string.Equals(storedClientState, BatchClientState.EndOfResponse.ToString()) || string.Equals(storedClientState, BatchClientState.Closed.ToString()))
                        {
                            shouldExit = true;
                        }
                        else
                        {
                            HashEntry[] stateEntry = { new HashEntry(batchId, state.ToString()) };
                            _cache.HashSet(hashKey, stateEntry);
                        }
                        CurrentQueueState = state;
                    }
                }
            }
           

            if (shouldExit)
            {
                //TraceHelper.TraceEvent(this.sessionid, TraceEventType.Information, "[AzureBatchJobMonitorEntry] Exit AzureBatchJobMonitor Entry");
                //clean up all queues info in Redis
                var batchClientKey = SessionConfigurationManager.GetRedisBatchClientStateKey(SessionId);
                _cache.KeyDelete(batchClientKey);
                var batchClientTotalTasksKey = SessionConfigurationManager.GetRedisBatchClientTotalTasksKey(SessionId, BatchId);
                _cache.KeyDelete(batchClientTotalTasksKey);
                var batchClientsFinishTasksKey = SessionConfigurationManager.GetRedisBatchClientFinishTasksKey(SessionId, BatchId);
                _cache.KeyDelete(batchClientsFinishTasksKey);
                this.Exit?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
