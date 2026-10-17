using System;

namespace BulletinBoard.Models.DailyMission2
{
    public class DailyMission2DayStatus
    {
        public DateTime Date { get; set; }

        public bool WordDone { get; set; }
        public bool PrayDone { get; set; }
        public bool QuizDone { get; set; }
        public bool ReciteDone { get; set; }

        public bool Word_ToMe { get; set; }
        public bool Word_ToParent { get; set; }

        public bool Pray_ToMe { get; set; }
        public bool Pray_ToParent { get; set; }
        public bool Pray_ToFriend { get; set; }

        public int QuizTryCntMax { get; set; }   // 0/1/2
        public bool QuizCorrect { get; set; }    // 정답 여부
    }


}
