using System;
using System.Collections.Generic;

namespace BulletinBoard.Models
{
    /// <summary>
    /// 말씀 정리 — 월별 일괄 입력(/JuManNa/MonthlyBulk) 한 달치.
    /// 주제 · 성경 · 장절만 다룬다.  있는 달이면 수정, 없는 날은 새로 넣는다.
    /// </summary>
    public class JuManNaMonthly
    {
        public string YYYYMM { get; set; }
        public List<JuManNaMonthlyDay> Days { get; set; } = new List<JuManNaMonthlyDay>();
    }

    public class JuManNaMonthlyDay
    {
        /// <summary>0 이면 아직 표에 없는 날 → 저장할 때 새로 넣는다</summary>
        public int Id { get; set; }
        public DateTime ExposeDate { get; set; }
        public string BibleSubject { get; set; }
        public string BibleText { get; set; }
        public int? FromJang { get; set; }
        public int? FromZul { get; set; }
        public int? ToJang { get; set; }
        public int? ToZul { get; set; }

        /// <summary>화면에서 값을 바꾼 날 — 있는 날은 이 표시가 있을 때만 저장한다</summary>
        public bool Changed { get; set; }
    }
}
