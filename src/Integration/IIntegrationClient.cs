using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Integration
{
    public interface IIntegrationClient
    {
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken, int timeoutMilliseconds = 5000) => throw new PluginException("Request failed");
    }
}
