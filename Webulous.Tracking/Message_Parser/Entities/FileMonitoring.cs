using Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Message_Parser.Entities
{
    public class FileMonitoring
    {
        public required string FileName { get; set; }
        public required DateTime TreatementDate { get; set; }
        public int? Speed { get; set; }
        public required FileStatus Status { get; set; }
        public int TreatedLogs { get; set; } = 0;
        public int SkippedLogs { get; set; } = 0;
    }
}
