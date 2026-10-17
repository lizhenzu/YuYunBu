using System;
using System.Collections.Generic;

namespace BulletinBoard.Models
{
    public class Event
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public QuestionTypeDataObject QuestionData { get; set; }
    }
    public class CalendarViewModel
    {
        public DateTime CurrentDate { get; set; }
        public List<Event> Events { get; set; }
    }

}
