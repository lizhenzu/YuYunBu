using System;
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class DeptGroupInfo
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; }
        public string ParentDeptCode { get; set; }
        public string DeptCode { get; set; }
        public string DeptName { get; set; }
    }
}
