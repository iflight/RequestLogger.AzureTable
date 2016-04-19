using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iflight.RequestLogger.AzureTable.Data
{
    public class LogEntity : TableEntity
    {
        public LogEntity(string partitionKey)
        {
            this.PartitionKey = partitionKey;
            this.RowKey =  string.Format("{0:D19}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks);
            this.RequestDateTime = DateTime.Now;
        }

        public DateTime RequestDateTime { get; set; }
        public string Path { get; set; }
        public string Query { get; set; }
        public int StatusCode { get; set; }
        public long TotalTime { get; set; }
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }
        public long RequestBodyLength { get; set; }
        public long ResponseBodyLength { get; set; }

    }
}
