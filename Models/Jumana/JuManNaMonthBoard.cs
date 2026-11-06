using System;
using System.Collections.Generic;

namespace BulletinBoard.Models
{
    /// <summary>말씀 정리 화면(달력형)에 보여 줄 하루치</summary>
    public class JuManNaBoardDay
    {
        public int Id { get; set; }
        public string Date { get; set; } = "";          // yyyy-MM-dd
        public string Ref { get; set; } = "";           // 고린도후서 4장 1절 ~ 4장 18절
        public string Subject { get; set; } = "";
        public string Synopsis { get; set; } = "";      // 줄거리
        public string Summary { get; set; } = "";       // 말씀 요약
        public List<string> Clips { get; set; } = new List<string>();     // 영상 구간 ("1:20 ~ 9:45")
        public List<string> Missing { get; set; } = new List<string>();   // 빠진 곳: 주제 · 줄거리 · 요약 · 영상
    }

    /// <summary>말씀 정리 화면(달력형) — 한 달</summary>
    public class JuManNaMonthBoard
    {
        public DateTime First { get; set; }
        public string YYYYMM { get; set; } = "";
        public List<JuManNaBoardDay> Days { get; set; } = new List<JuManNaBoardDay>();
    }
}
