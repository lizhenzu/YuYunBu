using System;
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class DeptRoleUserAssoc
    {
        public string RoleCode { get; set; }
        public string UserId { get; set; }
        
        public string RoleNM { get; set; }
        public int ClassYear { get; set; }
        public int ClassBan { get; set; }
    }
}

