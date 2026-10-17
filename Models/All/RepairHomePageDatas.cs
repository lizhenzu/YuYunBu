using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class RepairHomePageDatas : RepairHomePageData
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; }
    }
}
