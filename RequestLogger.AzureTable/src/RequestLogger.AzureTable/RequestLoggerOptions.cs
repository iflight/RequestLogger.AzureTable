namespace iflight.RequestLogger.AzureTable
{
    using System;

    public class RequestLoggerOptions
    {

        public RequestLoggerOptions()
        {
            azureConnectionString = "UseDevelopmentStorage=true;";
            azureTableName = "RequestLogger";
            interval = new TimeSpan(0, 0, 10);
            urlsPatterns = new string[] { };
        }

        private string azureConnectionString;

        public string AzureConnectionString
        {
            get { return azureConnectionString; }
            set
            {
                if(value == null)
                {
                    throw new ArgumentNullException("value");
                }

                azureConnectionString = value;
            }
        }

        private string azureTableName;

        public string AzureTableName
        {
            get { return azureTableName; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                azureTableName = value;
            }
        }

        private TimeSpan interval;

        public TimeSpan Interval
        {
            get { return interval; }
            set
            {
                interval = value;
            }
        }

        private string[] urlsPatterns;

        public string[] UrlsPatterns
        {
            get { return urlsPatterns; }
            set
            {
                if(value == null)
                {
                    throw new ArgumentNullException("value");
                }
                urlsPatterns = value;
            }
        }
    }
}
