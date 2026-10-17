using System;
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
	public class AdminPanelContents
    {
        public string RepreImage { get; set; }
        public string BackGroundColor { get; set; }
        public string KeyText { get; set; }
        public string ActionText { get; set; }
        public string ActionURL { get; set; }
	}
}
