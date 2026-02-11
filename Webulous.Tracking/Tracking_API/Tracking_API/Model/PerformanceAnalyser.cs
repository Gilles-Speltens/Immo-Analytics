namespace Tracking_API.Model
{
    public class PerformanceAnalyser
    {
        private int _actualLogs = 0;
        private TimeOnly _start = new TimeOnly();
        private TimeOnly _end = new TimeOnly();
        private int _nbOfFile = 1;
        private List<Queue> _queues = new List<Queue>();

        public PerformanceAnalyser()
        {
        }

        public void IncreaseLogs()
        {
            if (_actualLogs == 0)
            {
                _start = TimeOnly.FromDateTime(DateTime.Now);
            }
            _actualLogs++;
        }

        public void NewFile()
        {
            _nbOfFile++;
        }

        public void AddQueue(int sizeOfQueue, TimeSpan time)
        {
            _queues.Add(new Queue(sizeOfQueue, time));
            WriteData();
        }

        public void WriteData()
        {
            _end = TimeOnly.FromDateTime(DateTime.Now);
            TimeSpan elapsed = _end - _start;
            string queuesString = string.Join("\n", _queues.Select(q =>
                $"Size: {q.sizeOfQueue}, Time: {q.timeToFlushQueue}"
            ));
            Console.Write($@"
----- DATA -----
Duration : {elapsed.TotalSeconds}
Number of processed logs : {_actualLogs}
Number of flush : {_queues.Count}
Number of files : {_nbOfFile}
Queues :
{queuesString}

                ");
        }
    }
    public record Queue(
        int sizeOfQueue,
        TimeSpan timeToFlushQueue);
}
