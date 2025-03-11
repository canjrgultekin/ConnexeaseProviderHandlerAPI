using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaConsumerWorkerService.Models
{
    public class EventData
    {
        public string Provider { get; set; }
        public string ProjectName { get; set; }
        public string SessionId { get; set; }
        public string CustomerId { get; set; }
        public string ActionType { get; set; }
        public object Data { get; set; }
    }
}
