using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Telepathy.Frontend.MessagePersist;
using NsqSharp;
using StackExchange.Redis;

namespace Microsoft.Telepathy.Frontend.Services
{
    using Microsoft.Telepathy.ProtoBuf;

    public class FrontendBatchService : FrontendBatch.FrontendBatchBase
    {
        public FrontendBatchService()
        {
        }

        public override async Task<Empty> EndTasks(ClientEndOfTaskRequest request, ServerCallContext context)
        {
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            var channel = GrpcChannel.ForAddress(Configuration.SessionServiceAddress, new GrpcChannelOptions() { HttpHandler = httpClientHandler });
            var sessionSvcClient = new SessionManager.SessionManagerClient(channel);
            await sessionSvcClient.ClientEndOfTaskAsync(request);

            IDatabase cache = CommonUtility.Connection.GetDatabase();
            var topicName = GetQueueName(request.BatchClientInfo.SessionId, request.BatchClientInfo.ClientId) + ":totalNum";
            await cache.StringSetAsync(topicName, request.TotalRequestNumber);

            return new Empty();
        }

        public override async Task GetResults(BatchClientIdentity request, IServerStreamWriter<InnerResult> responseStream, ServerCallContext context)
        {
            using (IMessagePersist resultPersist = new RedisPersist(request))
            {
                while (!context.CancellationToken.IsCancellationRequested)
                {
                    var result = await resultPersist.GetResultAsync();

                    if (result != null)
                    {
                        await responseStream.WriteAsync(result);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        public override async Task<Empty> SendTask(IAsyncStreamReader<InnerTask> requestStream, ServerCallContext context)
        {
            using (IMessagePersist requestPersist = new NsqPersist())
            {
                await foreach (var request in requestStream.ReadAllAsync())
                {
                    Console.WriteLine("a message coming in");
                    await requestPersist.PutTaskAsync(request);
                }
            }

            return new Empty();
        }

        public override async Task<Empty> CloseBatch(CloseBatchClientRequest request, ServerCallContext context)
        {
            var channel = GrpcChannel.ForAddress(Configuration.SessionServiceAddress);
            var sessionSvcClient = new SessionManager.SessionManagerClient(channel);
            await sessionSvcClient.CloseBatchClientAsync(request);

            return new Empty();
        }

        private static string GetQueueName(string sessionId, string clientId)
        {
            var sb = new StringBuilder("{" + sessionId + "}");
            sb.Append(':');
            sb.Append(clientId);
            sb.Append(":response");
            return sb.ToString();
        }
    }
}
