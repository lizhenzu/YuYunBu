using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class LoginHistory
    {
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string UserName { get; set; }
        public string requestIP { get; set; }
    }
}
 