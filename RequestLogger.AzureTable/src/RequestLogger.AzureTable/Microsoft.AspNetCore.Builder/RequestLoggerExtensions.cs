namespace Microsoft.AspNetCore.Builder
{
    using AspNetCore.Builder;
    using iflight.RequestLogger.AzureTable;
    using Microsoft.Extensions.Logging;
    using System;
    using Microsoft.Extensions.Options;

    public static class RequestLoggerExtensions
    {
        /// <summary>
        /// Register RequestLoggerMiddleware
        /// </summary>
        /// <param name="configureOptions">RequestLogger options</param>
        /// <returns></returns>
        public static IApplicationBuilder UseRequestLogger(this IApplicationBuilder builder, RequestLoggerOptions options = null)
        {

            return builder.UseMiddleware<RequestLoggerMiddleware>(Options.Create(options ?? new RequestLoggerOptions()));
        }
    }
}
