using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class WeeklyCompositeData
    { 
        public WeeklyWordToRead WeeklyWordToReadData { get; set; }
         
        public WeeklyCompositeData()
        { 
            WeeklyWordToReadData = new WeeklyWordToRead();
        }
    }
}
