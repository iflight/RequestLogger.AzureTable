namespace Microsoft.AspNet.Builder
{
    using iflight.RequestLogger.AzureTable;

    public static class RequestLoggerExtensions
    {
        public static IApplicationBuilder UseRequestLogger(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggerMiddleware>();
        }

        public static IApplicationBuilder UseRequestLogger(this IApplicationBuilder builder, string azureConnectionString, string azureTableName, string[] urlsPatterns = null )
        {

            return builder.UseMiddleware<RequestLoggerMiddleware>(new object[] { azureConnectionString, azureTableName, urlsPatterns });
        }
    }
}
