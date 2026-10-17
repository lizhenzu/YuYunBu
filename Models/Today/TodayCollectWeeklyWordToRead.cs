using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class TodayCollectWeeklyWordToRead
    {
        public WeeklyWordToRead WeeklyWordToReadData { get; set; }
        public eBibles WeekeBiblesDataToRead { get; set; }
        public eBibles WeekeBiblesDataToRead_1 { get; set; }
        public eBibles WeekeBiblesDataToRecite { get; set; }
        public TodayCheckCompositeData TodayCheckCompositeData { get; set; }
        public List<UploadData> UploadDatas { get; set; }
        public List<UploadData> UploadDatasWelCome { get; set; }
        public List<UploadData> UploadDatasTeacher { get; set; }

        public TodayCollectWeeklyWordToRead()
        {
            WeeklyWordToReadData = new WeeklyWordToRead();   
            WeekeBiblesDataToRead = new eBibles();
            WeekeBiblesDataToRecite = new eBibles();
            TodayCheckCompositeData = new TodayCheckCompositeData();
            UploadDatas = new List<UploadData>();
        }
    }
}
