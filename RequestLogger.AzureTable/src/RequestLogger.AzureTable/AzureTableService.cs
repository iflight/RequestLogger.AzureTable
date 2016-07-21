using iflight.RequestLogger.AzureTable.Data;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        private Mutex mtx = new Mutex();

        private List<LogEntity> logEntities;

        private TimeSpan saveInterval;

        private AzureTableService()
        {
            throw new NotImplementedException("Use Init method!");
        }

        private AzureTableService(string ConnectionString, string tableName, ILoggerFactory loggerFactory, TimeSpan interval)
        {
            logger = loggerFactory.CreateLogger<AzureTableService>();
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionString);
            var table = storageAccount.CreateCloudTableClient();
            cloudTable = table.GetTableReference(tableName);
            logEntities = new List<LogEntity>();
            saveInterval = interval;
            azureTableTask = new Task(async () => await SaveLoop());
            azureTableTask.Start();
        }

        public async static void Init(string ConnectionString, string tableName, ILoggerFactory loggerFactory, TimeSpan interval)
        {
            if (instance != null)
            {
                return;
            }
            instance = new AzureTableService(ConnectionString, tableName, loggerFactory, interval);
            await instance.cloudTable.CreateIfNotExistsAsync();

        }

        public void Log(string request, string response, string path, string query, long requestLenght, long responseLenght, int statusCode, long totalTime, string exception, string ip = "")
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
            entity.Ip = ip;

            mtx.WaitOne();
            logEntities.Add(entity);
            mtx.ReleaseMutex();
        }

        private async Task SaveLoop()
        {
            while (true)
            {
                logger.LogDebug("Try to save log to Azure Table");
                try
                {
                    if (logEntities.Any())
                    {
                        mtx.WaitOne();
                        var entities = logEntities.Take(100).ToList();
                        logEntities.RemoveRange(0, entities.Count);
                        mtx.ReleaseMutex();

                        var entitiesGroups = entities.GroupBy(x => x.PartitionKey);
                        foreach (var entityGroup in entitiesGroups)
                        {
                            TableBatchOperation batch = new TableBatchOperation();
                            foreach (var entity in entityGroup)
                            {
                                batch.Add(TableOperation.Insert(entity));
                            }

                            await cloudTable.ExecuteBatchAsync(batch);
                        }

                        logger.LogInformation(String.Format("{0} entities saved to Azure Table in {1} batches", entities.Count, entitiesGroups.Count()));
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e.Message + "\r\n" + e.StackTrace);
                }
                System.Threading.Thread.Sleep(saveInterval);
            }
        }
    }
}
