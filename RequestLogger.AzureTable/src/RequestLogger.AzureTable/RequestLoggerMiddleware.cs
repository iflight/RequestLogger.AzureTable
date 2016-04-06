namespace iflight.RequestLogger.AzureTable
{
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.AspNet.Builder;
    using Microsoft.AspNet.Http;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
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
                    var stream = context.Response.Body;
                    var buffer = new MemoryStream();
                    context.Response.Body = buffer;

                    
                    string requestBody = new StreamReader(context.Request.Body).ReadToEnd();
                    string path = context.Request.Path;
                    string query = context.Request.QueryString.HasValue ? context.Request.QueryString.ToString() : "";
                    long requestLenght = context.Request.ContentLength.HasValue ? context.Request.ContentLength.Value : 0;

                    await _next(context);
                    sw.Stop();

                    buffer.Seek(0, SeekOrigin.Begin);
                    var reader = new StreamReader(buffer);
                    string responseBody = await reader.ReadToEndAsync();

                    buffer.Seek(0, SeekOrigin.Begin);
                    await buffer.CopyToAsync(stream);

                    long responseLenght = context.Response.ContentLength.HasValue ? context.Response.ContentLength.Value : buffer.Length;
                    int code = context.Response.StatusCode;

                    await AzureTableService.Instance.Log(requestBody, responseBody,path,query, requestLenght, responseLenght, code, sw.ElapsedMilliseconds);

                    _logger.LogInformation("Log to Azure complite");
                }
                catch (Exception e)
                {
                    _logger.LogError(e.StackTrace +"\r\n"+e.Message);
                }
            }
            else
            {
                await _next(context);
            }


        }
    }
}
