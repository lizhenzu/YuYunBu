using System;
using System.Collections.Generic;

namespace BulletinBoard.Models
{
    /// <summary>
    /// 말씀 정리 — 영상 구간 편집(/JuManNa/SegmentEditor) 한 달치.
    /// 영상은 장마다 하나(eBibleYouTuBu).  구간 1 ~ 5 = FromJang 부터 차례로 한 장씩.
    /// </summary>
    public class JuManNaSegMonth
    {
        public string YYYYMM { get; set; }
        public List<JuManNaSegDay> Days { get; set; } = new List<JuManNaSegDay>();
    }

    public class JuManNaSegDay
    {
        public int Id { get; set; }
        public DateTime ExposeDate { get; set; }
        public string BibleText { get; set; }
        public int FromJang { get; set; }
        public int FromZul { get; set; }
        public int ToJang { get; set; }
        public int ToZul { get; set; }
        public List<JuManNaSeg> Segs { get; set; } = new List<JuManNaSeg>();
    }

    public class JuManNaSeg
    {
        /// <summary>1 ~ 5 (컬럼 YouTuBuStartMM · YouTuBuStartMM2 … 의 번호)</summary>
        public int Slot { get; set; }
        public int Jang { get; set; }
        public string Url { get; set; }
        /// <summary>영상 전체 길이(초).  모르면 0</summary>
        public int LenSec { get; set; }
        /// <summary>저장된 시작(초).  0 = 영상 처음</summary>
        public int StartSec { get; set; }
        /// <summary>저장된 종료(초).  0 = 영상 끝</summary>
        public int EndSec { get; set; }
    }
}
