using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace iflight.RequestLogger.AzureTable
{
    public class LoggingHandler : DelegatingHandler
    {
        public LoggingHandler()
            : this(new HttpClientHandler())
        { }

        public LoggingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        { }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {


            string requestBody = request.Content != null ? await request.Content.ReadAsStringAsync() : "";
            string path = request.RequestUri.AbsoluteUri;
            string query = request.RequestUri.Query;
            long requestLenght = request.Content != null ? (await request.Content.ReadAsStreamAsync()).Length : 0;
            var sw = new Stopwatch();
            sw.Start();
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            sw.Stop();
            string responseBody = await response.Content.ReadAsStringAsync();

            long responseLenght = (await response.Content.ReadAsStreamAsync()).Length;
            int code = (int)response.StatusCode;

            await AzureTableService.Instance.Log(requestBody, responseBody, path, query, requestLenght, responseLenght, code, sw.ElapsedMilliseconds);
            return response;
        }
    }
}
