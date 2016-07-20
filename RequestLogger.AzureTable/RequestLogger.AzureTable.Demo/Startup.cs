using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using iflight.RequestLogger.AzureTable;

namespace RequestLogger.AzureTable.Demo
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseDeveloperExceptionPage();

            app.UseStaticFiles();
            //@"UseDevelopmentStorage=true;", "AzureLoggerDemo", new string[] { "demo" }, new TimeSpan(0, 0, 30)
            app.UseRequestLogger(new RequestLoggerOptions()
            {
                AzureConnectionString = "UseDevelopmentStorage=true;",
                AzureTableName = "AzureLoggerDemo",
                UrlsPatterns = new string[] { "demo" },
                Interval = new TimeSpan(0,0,30)           
            });
            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/fail/demo")
                {
                    throw new Exception();
                }
                if (context.Request.Path == "/fail/foobar")
                {
                    throw new Exception();
                }
                await next.Invoke();
            });


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
