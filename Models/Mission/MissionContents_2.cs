using System;
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
	public class MissionContents_2
    {
		public string BackGround { get; set; }
		public string MissionMessage { get; set; }
		public string MissionMessageA { get; set; }

        public string MissionText { get; set; }

        public string MissionURL { get; set; }
        public string MissionURL1 { get; set; }
        public string MissionURL2 { get; set; }

        public string MissionURL_Text { get; set; }
        public string MissionURL1_Text { get; set; }
        public string MissionURL2_Text { get; set; }


        public string ButtonStyle { get; set; }
        public int MissionCount { get; set; }
        public string Mainclass { get; set; }
        public TodayCheckCompositeData _todayCheckCompositeData { get; set; }


        public string missionTitle { get; set; }
        public string missionDesc { get; set; }
        public string missionButtonText { get; set; }
        public string missionButtonClass { get; set; }

        public MissionContents_2() {
			Mainclass = "mission";
            MissionURL = null;
            MissionURL1 = null;
            MissionURL2 = null;

            MissionURL_Text = null;
            MissionURL1_Text = null;
            MissionURL2_Text = null;
        }

    }
}
