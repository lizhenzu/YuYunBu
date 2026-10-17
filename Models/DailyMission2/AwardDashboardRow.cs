// Models/CategoryOptions.cs
using System;
using System.Collections.Generic;

namespace BulletinBoard.Models.DailyMission2
{
    public class AwardDashboardRow
    {
        public string UserName { get; set; } = "";
        public string? UserId { get; set; }
        public int cYear { get; set; }
        public int AwardLevel { get; set; }
        public string Status { get; set; } = "Eligible"; // Eligible / Selected / Delivered
        public int? SelectedProductID { get; set; }
        public int YearTotalPoint { get; set; }
        // ============================
        // ✅ 추가 필드
        // ============================ 
        public string? FullName { get; set; }
        public int? ClassYear { get; set; }
        public int? ClassBan { get; set; }

        public string? SelectedProductName { get; set; }
        public string? SelectedProductDescription { get; set; }
        public string? ProductImageUrl { get; set; }

        // ✅ 추가: 서버가 만들어서 뷰에 그냥 뿌릴 최종 표시용
        public bool IsStudent { get; set; }
        public bool IsTeacher { get; set; }
        public string? RoleCode { get; set; }         // ex) T1002
        public string? RoleName { get; set; }         // CodeData.MinorNM
        public string? DeptCode { get; set; }         // view_All_Person_2026.DeptCode
        public string DisplayGroupText { get; set; } = "학년/반 없음"; // ✅ 뷰에 뿌릴 값(학생이면 학년반, 교사면 역할명)


    }


}
