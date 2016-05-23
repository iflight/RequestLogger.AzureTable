namespace iflight.RequestLogger.AzureTable
{
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.IO;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Diagnostics;
    using Microsoft.AspNetCore.Http;
    public class RequestLoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private string[] _urlsPatterns;

        public RequestLoggerMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, string azureConnectionString, string azureTableName, string[] urlsPatterns = null, TimeSpan? interval = null)
        {
            _next = next;
            _urlsPatterns = urlsPatterns;
            _logger = loggerFactory.CreateLogger<RequestLoggerMiddleware>();

            AzureTableService.Init(azureConnectionString, azureTableName, loggerFactory, interval);
        }

        public async Task Invoke(HttpContext context)
        {
            var sw = new Stopwatch();


            string requestUrl = context.Request.Host + context.Request.Path;
            bool needLogged = false;
            if (_urlsPatterns != null && _urlsPatterns.Length > 0)
            {
                foreach (var pattern in _urlsPatterns)
                {
                    if (Regex.Match(requestUrl, pattern).Success)
                    {
                        needLogged = true;
                        break;
                    };
                }
            }
            else
            {
                needLogged = true;
            }

            if (needLogged)
            {

                _logger.LogInformation("Log to RequestLogger...");

                var requestBuffer = new MemoryStream();
                await context.Request.Body.CopyToAsync(requestBuffer);

                requestBuffer.Seek(0, SeekOrigin.Begin);
                var requestReader = new StreamReader(requestBuffer);
                string requestBody = await requestReader.ReadToEndAsync();
                requestBuffer.Seek(0, SeekOrigin.Begin);
                context.Request.Body = requestBuffer;

                string path = context.Request.Host + context.Request.Path;
                string query = context.Request.QueryString.HasValue ? context.Request.QueryString.ToString() : "";
                long requestLenght = context.Request.ContentLength.HasValue ? context.Request.ContentLength.Value : requestBuffer.Length;

                var responseStream = context.Response.Body;
                var responseBuffer = new MemoryStream();
                context.Response.Body = responseBuffer;

                string responseBody = string.Empty;
                long responseLenght = 0;
                int code = 0;

                string exception = string.Empty;
                try
                {
                    sw.Start();
                    await _next(context);
                    sw.Stop();

                    responseBuffer.Seek(0, SeekOrigin.Begin);
                    var reader = new StreamReader(responseBuffer);
                    responseBody = await reader.ReadToEndAsync();
                    responseBuffer.Seek(0, SeekOrigin.Begin);
                    await responseBuffer.CopyToAsync(responseStream);
                    
                    responseLenght = context.Response.ContentLength.HasValue ? context.Response.ContentLength.Value : responseBuffer.Length;
                    code = context.Response.StatusCode;
                }
                catch (Exception e)
                {
                    exception = e.Message + "\r\n" + e.StackTrace;

                    _logger.LogInformation(e.Message + "\r\n" + e.StackTrace);
                }

                AzureTableService.Instance.Log(requestBody, responseBody, path, query, requestLenght, responseLenght, code, sw.ElapsedMilliseconds, exception);

                _logger.LogInformation("Log to RequestLogger complete");

            }
            else
            {
                await _next(context);
            }


        }
    }
}
