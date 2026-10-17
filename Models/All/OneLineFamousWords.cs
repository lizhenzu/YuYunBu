using System;
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class OneLineFamousWords : OneLineFamousWord
    {       
        [Display(Name = "No.")]
        public long sequenceNo { get; set; }
    }
}
