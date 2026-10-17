using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class BessingACoupleData
    {
        public int Id { get; set; }
        public int PageNo { get; set; }

        public string PrayTitle { get; set; }
        public string PrayText { get; set; }
        public string PrayHolyWord { get; set; } 
        public DateTime UpdateDT { get; set; }


        public int visited { get; set; }

        // 검증 (2026-10-11) — 표에 칸이 아직 없으면 항상 false / null (ControlBessingACouple 이 채운다)
        public bool IsVerified { get; set; }
        public DateTime? VerifiedDT { get; set; }
        public string VerifiedBy { get; set; }

    }
}
