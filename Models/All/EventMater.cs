using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class EventMater
    {
        public int EventId { get; set; }
         
        public string EventTitle { get; set; }
        public string EventDesc { get; set; }
        public string EventPlace { get; set; }
        public string processStatus { get; set; }
        public string applyYN { get; set; }
        public string URL { get; set; }
        public string URLDesc { get; set; }
        public string ForWhom { get; set; }        
        public TimeSpan? EventTimeFrom { get; set; }
        public TimeSpan? EventTimeTo { get; set; }
        public DateTime EventDayFrom { get; set; }
        public DateTime EventDayTo { get; set; }
        public int EventCountLimitStudent { get; set; }
        public int EventCountLimitTeacher { get; set; }        
    }
}
