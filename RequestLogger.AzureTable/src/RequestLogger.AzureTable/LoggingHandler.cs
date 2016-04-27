using System;
using System.Diagnostics;
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
            string path = String.Format("{0}{1}", request.RequestUri.Authority, request.RequestUri.AbsolutePath);
            string query = request.RequestUri.Query;
            long requestLenght = request.Content != null ? (await request.Content.ReadAsStreamAsync()).Length : 0;
            var sw = new Stopwatch();
            HttpResponseMessage response = null;
            string responseBody = string.Empty;
            long responseLenght = 0;
            int code = 0;
            string exception = string.Empty;

            try
            {
                sw.Start();
                response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                sw.Stop();
                responseBody = await response.Content.ReadAsStringAsync();
                responseLenght = (await response.Content.ReadAsStreamAsync()).Length;
                code = (int)response.StatusCode;
            }
            catch (Exception e)
            {
                exception = e.Message + "\r\n" + e.StackTrace;
            }
            await AzureTableService.Instance.Log(requestBody, responseBody, path, query, requestLenght, responseLenght, code, sw.ElapsedMilliseconds, exception);
            return response;
        }
    }
}
