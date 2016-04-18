# RequestLogger.AzureTable

##How to use

In Startup.cs:

```cs
        public async void Configure(IApplicationBuilder app,ILoggerFactory loggerfactory)
        {
            ...
            app.UseRequestLogger( "AzureStorageConnectionString", "AzureTableName", new string[] { "observeblePath1","observeblePath2" });
            ...
        }
  ```
  
##Use with System.Net.Http.HttpClient
  
  ```cs
  HttpClient client = new HttpClient(new iflight.RequestLogger.AzureTable.LoggingHandler(new HttpClientHandler()));
  string strings = await client.GetStringAsync("https://www.example.com/");
  ```
