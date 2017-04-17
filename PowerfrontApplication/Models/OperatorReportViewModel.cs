using System;
using System.Collections.Generic;

namespace PowerfrontApplication.Models
{
    public class OperatorReportViewModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int ProactiveSent { get; set; }
        public int ProactiveAnswered { get; set; }
        public int ProactiveResponseRate { get; set; }
        public int ReactiveReceived { get; set; }
        public int ReactiveAnswered { get; set; }
        public int ReactiveResponseRate { get; set; }
        public string TotalChatLength { get; set; }
        public string AverageChatLength { get; set; }
    }

    public class OperatorReportItems
    {
        public OperatorReportItems()
        {
            OperatorProductivity = new List<OperatorReportViewModel>();
        }
        public ICollection<OperatorReportViewModel> OperatorProductivity { get; set; }
    }

    public class DataFilters
    {
        public string PreDefinedDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Website { get; set; }
        public string Device { get; set; }
    }

}

