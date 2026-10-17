using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class SurveyDatas : SurveyData
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; }
    }
}
