using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class MissionMenuItem
    {
        public string Role { get; set; }
        public string MissionText { get; set; }
        public string MissionMessage { get; set; }
        public string MissionURL1 { get; set; }
        public string MissionURL2 { get; set; }
        public string MissionURL3 { get; set; }
        public string MissionURL4 { get; set; }
        public string MissionURL5 { get; set; }
        public string MissionURL6 { get; set; }
        public string MissionURL1_Text { get; set; }
        public string MissionURL2_Text { get; set; }
        public string MissionURL3_Text { get; set; }
        public string MissionURL4_Text { get; set; }
        public string MissionURL5_Text { get; set; }
        public string MissionURL6_Text { get; set; }
        public string BackGround { get; set; }
		public string ButtonStyle { get; set; }
		public string Header { get; set; }
		public int DisplayOrder { get; set; }
    }

}
