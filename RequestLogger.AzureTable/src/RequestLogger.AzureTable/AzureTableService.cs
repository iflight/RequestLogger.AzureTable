using iflight.RequestLogger.AzureTable.Data;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Diagnostics;

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

        private CloudTable cloudTable;

        private Task azureTableTask;

        private AzureTableService()
        {
            throw new NotImplementedException("Use Init method!");
        }

        private AzureTableService(string ConnectionString, string tableName)
        {
            CloudStorageAccount mySA = CloudStorageAccount.Parse(ConnectionString);
            var table = mySA.CreateCloudTableClient();
            cloudTable = table.GetTableReference(tableName);
        }

        public async static void Init(string ConnectionString, string tableName)
        {
            if (instance != null)
            {
                return;
            }
            instance = new AzureTableService(ConnectionString, tableName);
            await instance.cloudTable.CreateIfNotExistsAsync();
        }

        public async Task Log(string request, string response, string path, string query, long requestLenght, long responseLenght, int statusCode, long totalTime)
        {

            LogEntity entity = new LogEntity(path.Trim('/').Replace('/','-'));
            entity.RequestBody = request;
            entity.ResponseBody = response;
            entity.Path = path;
            entity.Query = query;
            entity.RequestBodyLength = requestLenght;
            entity.ResponseBodyLength = responseLenght;
            entity.TotalTime = totalTime;
            entity.StatusCode = statusCode;

            TableOperation insertOperation = TableOperation.Insert(entity);
            await cloudTable.ExecuteAsync(insertOperation);

        }
    }
}
