using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace BulletinBoard.Models
{
    public class ExpectedAward : view_Ranking_By_Person
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; }
        public int DC_Current { get; set; }
        public int CurrentAward { get; set; }
        public int DC_Projected { get; set; }
        public int ProjectedAward { get; set; }
        public bool IsTotal { get; set; } 


        public string ProfilePicturePath { get; set; }
    }
}
 