using iflight.RequestLogger.AzureTable.Data;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace iflight.RequestLogger.AzureTable
{
    public class AzureTableService
    {
        private static AzureTableService instance;

        public static AzureTableService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureTableService();
                }
                return instance;
            }

        }

        private ILogger logger;

        private CloudTable cloudTable;

        private Task azureTableTask;

        private ConcurrentQueue<TableOperation> queue;

        private AzureTableService()
        {
            throw new NotImplementedException("Use Init method!");
        }

        private AzureTableService(string ConnectionString, string tableName, ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<AzureTableService>();
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionString);
            var table = storageAccount.CreateCloudTableClient();
            cloudTable = table.GetTableReference(tableName);
            queue = new ConcurrentQueue<TableOperation>();
            azureTableTask = new Task(async () => await SaveLoop());
            azureTableTask.Start();
        }

        public async static void Init(string ConnectionString, string tableName, ILoggerFactory loggerFactory)
        {
            if (instance != null)
            {
                return;
            }
            instance = new AzureTableService(ConnectionString, tableName, loggerFactory);
            await instance.cloudTable.CreateIfNotExistsAsync();


        }

        public void Log(string request, string response, string path, string query, long requestLenght, long responseLenght, int statusCode, long totalTime, string exception)
        {

            LogEntity entity = new LogEntity(path.Trim('/').Replace('/', '-'));
            entity.RequestBody = request;
            entity.ResponseBody = response;
            entity.Path = path;
            entity.Query = query;
            entity.RequestBodyLength = requestLenght;
            entity.ResponseBodyLength = responseLenght;
            entity.TotalTime = totalTime;
            entity.StatusCode = statusCode;
            entity.Exception = exception;


            TableOperation insertOperation = TableOperation.Insert(entity);
            // await cloudTable.ExecuteAsync(insertOperation);
            queue.Enqueue(insertOperation);
        }

        private async Task SaveLoop()
        {
            while (true)
            {
                try {
                    if (queue.Any())
                    {
                        TableOperation operation = null;
                        queue.TryDequeue(out operation);
                        if (operation != null)
                        {
                            await cloudTable.ExecuteAsync(operation);
                            logger.LogInformation("Entity saved to Azure Table");
                        }
                    }
                }catch(Exception e)
                {
                    logger.LogError(e.Message + "\r\n" + e.StackTrace);
                }
            }
        }
    }
}
