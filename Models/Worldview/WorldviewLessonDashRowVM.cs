using System;

namespace BulletinBoard.ViewModels.Worldview
{
    // 과별 대시보드 한 줄(1과,2과,...)
    public class WorldviewLessonDashRowVM
    {
        public int LessonNo { get; set; }

        // 제출 여부
        public bool Submitted { get; set; }

        // 정답(올정답) 여부
        public bool IsAllCorrect { get; set; }

        // 점수들
        public int BasePoint { get; set; }
        public int BonusPoint { get; set; }
        public int TotalPoint { get; set; }

        // 제출일(선택)
        public DateTime? SubmitDT { get; set; }
    }

 
}
