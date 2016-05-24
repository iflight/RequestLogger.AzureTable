# RequestLogger.AzureTable

##How to use

In Startup.cs:

```cs
        public async void Configure(IApplicationBuilder app,ILoggerFactory loggerfactory)
        {
            ...
            app.UseRequestLogger(new RequestLoggerOptions()
            {
                AzureConnectionString = "UseDevelopmentStorage=true;",
                AzureTableName = "AzureLoggerDemo",
                UrlsPatterns = new string[] { "demo" },
                Interval = new TimeSpan(0,0,30)   
                
            });
            ...
        }
  ```
  
##Use with System.Net.Http.HttpClient
  
  ```cs
  HttpClient client = new HttpClient(new iflight.RequestLogger.AzureTable.LoggingHandler(new HttpClientHandler()));
  string strings = await client.GetStringAsync("https://www.example.com/");
  ```
