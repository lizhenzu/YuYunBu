namespace BulletinBoard.Models
{
    public class MissionContents
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

		public MissionContents() {
			Mainclass = "mission";
            MissionURL1 = null;
            MissionURL2 = null;
            MissionURL = null;

            MissionURL1_Text = null;
            MissionURL2_Text = null;
            MissionURL_Text = null;
        }

    }
}
