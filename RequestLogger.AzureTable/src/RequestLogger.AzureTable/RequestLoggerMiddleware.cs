namespace iflight.RequestLogger.AzureTable
{
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.AspNet.Builder;
    using Microsoft.AspNet.Http;
    using System.IO;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Diagnostics;

    public class RequestLoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private string[] _urlsPatterns;

        public RequestLoggerMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, string azureConnectionString, string azureTableName, string[] urlsPatterns = null)
        {
            _next = next;
            _urlsPatterns = urlsPatterns;
            _logger = loggerFactory.CreateLogger<RequestLoggerMiddleware>();

            AzureTableService.Init(azureConnectionString, azureTableName);
        }

        public async Task Invoke(HttpContext context)
        {
            var sw = new Stopwatch();
            sw.Start();

            string requestUrl = context.Request.Host + context.Request.Path;
            bool needLogged = false;
            if (_urlsPatterns != null && _urlsPatterns.Length > 0)
            {
                foreach (var pattern in _urlsPatterns)
                {
                    if (Regex.Match(requestUrl, pattern).Success)
                    {
                        needLogged = true;
                    };
                    if (needLogged)
                    {
                        break;
                    }
                }
            }
            else
            {
                needLogged = true;
            }
            if (needLogged)
            {
                try
                {
                    _logger.LogInformation("Log to Azure...");

                    var requestBuffer = new MemoryStream();
                    await context.Request.Body.CopyToAsync(requestBuffer);
                    
                    var responseStream = context.Response.Body;
                    var responseBuffer = new MemoryStream();
                    context.Response.Body = responseBuffer;

                    requestBuffer.Seek(0, SeekOrigin.Begin);
                    var requestReader = new StreamReader(requestBuffer);
                    string requestBody = await requestReader.ReadToEndAsync();
                    requestBuffer.Seek(0, SeekOrigin.Begin);
                    context.Request.Body = new MemoryStream(requestBuffer.ToArray());

                    string path = context.Request.Host + context.Request.Path;
                    string query = context.Request.QueryString.HasValue ? context.Request.QueryString.ToString() : "";
                    long requestLenght = context.Request.ContentLength.HasValue ? context.Request.ContentLength.Value : responseBuffer.Length;

                    await _next(context);
                    sw.Stop();

                    responseBuffer.Seek(0, SeekOrigin.Begin);
                    var reader = new StreamReader(responseBuffer);
                    string responseBody = await reader.ReadToEndAsync();
                    responseBuffer.Seek(0, SeekOrigin.Begin);
                    await responseBuffer.CopyToAsync(responseStream);

                    long responseLenght = context.Response.ContentLength.HasValue ? context.Response.ContentLength.Value : responseBuffer.Length;
                    int code = context.Response.StatusCode;

                    await AzureTableService.Instance.Log(requestBody, responseBody, path, query, requestLenght, responseLenght, code, sw.ElapsedMilliseconds);

                    _logger.LogInformation("Log to Azure complete");
                }
                catch (Exception e)
                {
                    _logger.LogError(e.StackTrace + "\r\n" + e.Message);
                }
            }
            else
            {
                await _next(context);
            }


        }
    }
}
