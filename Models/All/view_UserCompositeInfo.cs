using System;
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class view_UserCompositeInfo
    {        

        public string UserName { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string FullName { get; set; }

        public string RoleCode { get; set; }
        public string RoleNM { get; set; }
        public string DeptCode { get; set; }
        public string DeptNM { get; set; }
        public int readBibleAllCount { get; set; }
        public int ClassYear { get; set; }
        public int ClassBan { get; set; }

        public string UserId { get; set; }
        public string ProfilePicturePath { get; set; }        
    }
}
