using System.Text.Json.Serialization;

namespace ChristmasLightServer
{
    public class DataCounterReport
    {
        public DataCounterReport(DateTime time, int count)
        {
            timeStamp = string.Format($"{time.Year:0000}-{time.Month:00}-{time.Day:00}T{time.Hour:00}:{time.Minute:00}:{time.Second:00}+00:00");
            byteCount = count;
        }

        [JsonInclude]
        [JsonPropertyName("@timestamp")]
        public string timeStamp { get; private set; }

        [JsonInclude]
        [JsonPropertyName("bytecount")]
        public int byteCount { get; private set; } 
    }

    public class DataCounter : IDataCounter
    {
        private readonly Mutex _mutex;
        private readonly ILogger<DataCounter> _logger;
        private readonly EventWaitHandle _reportEvent;
        private readonly Thread _workerThread;
        private int _byteCount;

        public DataCounter(ILogger<DataCounter> logger)
        {
            _mutex = new Mutex(false);
            _logger = logger;
            _reportEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
            _workerThread = new Thread(new ThreadStart(() => Worker()));
            _byteCount = 0;
        }
        public void logBytes(int byteCount)
        {
            _mutex.WaitOne();
            _byteCount += byteCount;
            _mutex.ReleaseMutex();
        }

        public void startReporting()
        {
            if (_workerThread.ThreadState == ThreadState.Unstarted)
            {
                _workerThread.Start();
            }
        }

        private async Task reportData()
        {
            int bytesToReport;
            _mutex.WaitOne();
            bytesToReport = _byteCount;
            _byteCount = 0;
            _mutex.ReleaseMutex();

            if (bytesToReport > 0)
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://192.168.0.10:9200"); //local elasticsearch

                var time = DateTime.UtcNow;
                var url = string.Format($"datacounter{time.Year:0000}{time.Month:00}{time.Day:00}/_doc");

                var content = JsonContent.Create(new DataCounterReport(time, bytesToReport));

                var response = await client.PostAsync(url, content);
               
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed sending data counter report");
                }
            }
        }

        private void Worker()
        {
            while (true)
            {
                _reportEvent.WaitOne(TimeSpan.FromMinutes(5.0));
                Task.Run(reportData);
            }
        }
    }
}
