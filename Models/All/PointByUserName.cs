using System;
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    //public class view_Ranking_By_Person
    //{
    //    public string UserName { get; set; }
    //    public string FirstName { get; set; }
    //    public string LastName { get; set; }
    //    public string FullName { get; set; }
        
    //    public int DC { get; set; }
    //    public int DC_Total { get; set; }
    //    public int ClassYear { get; set; }
    //    public int ClassBan { get; set; }
    //    public DateTime minclickDate { get; set; }
    //    public DateTime maxclickDate { get; set; }
    //    public string UserId { get; set; }
    //    public string RoleNM { get; set; }
    //    public string DeptNM { get; set; }
    //    public string RoleCode { get; set; }
    //    public string DeptCode { get; set; }
    //}

    public class PointByUserName : view_Ranking_By_Person
    {
        [Display(Name = "No.")]
        public long sequenceNo { get; set; }
        public string ProfilePicturePath { get; set; }
        public int AwardLevel { get; set; }
        public int AwardLevel1000Value { get; set; }
        public int DC_Total { get; set; }
        public int Total { get; set; }


    }
}
 