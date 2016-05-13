namespace Microsoft.AspNet.Builder
{
    using iflight.RequestLogger.AzureTable;
    using System;
    public static class RequestLoggerExtensions
    {
        /// <summary>
        /// Register RequestLoggerMiddleware
        /// </summary>
        /// <param name="azureConnectionString">Connection string to Azute Table Storage</param>
        /// <param name="azureTableName">Table name in Azure Table Storage</param>
        /// <param name="urlsPatterns">Url patterns what must be logged (regex expressions allowed)</param>
        /// <param name="interval">Interval between saving logs to Azure Table </param>
        /// <returns></returns>
        public static IApplicationBuilder UseRequestLogger(this IApplicationBuilder builder, string azureConnectionString, string azureTableName, string[] urlsPatterns = null, TimeSpan? interval = null)
        {

            return builder.UseMiddleware<RequestLoggerMiddleware>(new object[] { azureConnectionString, azureTableName, urlsPatterns, interval });
        }
    }
}
