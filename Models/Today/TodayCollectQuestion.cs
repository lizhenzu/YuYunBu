using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class TodayCollectQuestion
    {
        public QuestionTypeDataObjects QuestionTypeDataObjectsData { get; set; }
        public TodayCheckCompositeData TodayCheckCompositeData { get; set; }
        public QuestionProcessStatus questionProcessStatus { get; set; }
        public TodayWordToRead TodayWordToReadData { get; set; }

        public TodayCollectQuestion()
        {
            QuestionTypeDataObjectsData = new QuestionTypeDataObjects();
            TodayCheckCompositeData = new TodayCheckCompositeData();
            questionProcessStatus = new QuestionProcessStatus();
            TodayWordToReadData = new TodayWordToRead();
        }
    }
}
