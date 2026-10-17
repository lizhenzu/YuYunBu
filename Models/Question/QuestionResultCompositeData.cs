using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class QuestionResultCompositeData
    {
        public QuestionTypeDataObject QuestionTypeDataObjectData { get; set; }
        
        public QuestionResultCompositeData()
        {
            QuestionTypeDataObjectData = new QuestionTypeDataObject();
        }
    }
}
