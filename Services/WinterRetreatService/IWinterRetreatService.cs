using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BulletinBoard.Services.Abstract
{
    public interface IVBSParticipationSurveyService
    {
        // ✅ 참여 여부(참석 입력 여부) 판단
        Task<bool> HasParticipatedAsync(string UserName);

        /// <summary>
        /// 홈 가이드 팝업 노출 여부
        /// - 1순위: 참석/참여 입력이 있으면 팝업은 의미 없음 → false
        /// - 2순위: (참여 입력이 없을 때만) UserName 기준 유예 상태(NextShowAt)로 결정
        /// </summary>
        Task<bool> ShouldShowGuideAsync(string userName, string? UserName);

        /// <summary>
        /// 홈 가이드 팝업 유예 (UserName 기준)
        /// - days=0 : "다음에 할래요" (저장 안 함/즉시 다시 뜰 수 있음)
        /// - days=3 : "3일후 할래요"
        /// - days=7 : "7일후 할래요"
        /// </summary>
        Task DeferGuideAsync(string userName, int days);

        /// <summary>
        /// 내 슬롯(AM/PM) 저장/업데이트
        /// </summary>
        Task UpsertSlotAsync(
            string UserName,
            string fullName,
            string roleCode,
            DateTime date,
            string slot,
            bool? isAttend,
            string requestIp,
            string? updatedByUserName,
            string? statusNote = null
        );

        /// <summary>
        /// 전일 참석/미참석 (6칸 저장 + 완료판정 기록)
        /// </summary>
        Task SetAllAsync(string UserName, string fullName, string roleCode, bool isAttend, string requestIp);

        /// <summary>
        /// 전체 날짜 상태 메모 일괄 저장 (CONDITIONAL / ABSENT_CONFIRM / UNDECIDED)
        /// </summary>
        Task SetAllStatusNoteAsync(string UserName, string fullName, string roleCode, string statusNote, string requestIp);

        /// <summary>
        /// 내 슬롯 조회 (IsAttend + StatusNote 포함)
        /// </summary>
        Task<Dictionary<(DateTime Date, string Slot), (bool? IsAttend, string? StatusNote)>> GetMySlotsAsync(string UserName);

        /// <summary>
        /// 관리자 표 데이터 생성 (미응답 포함)
        /// </summary>
        Task<VBSParticipationSurveyAdminVM> GetAdminVMAsync(IEnumerable<Models.UserInfo> users, string DeptCode);
    }

    public class VBSParticipationSurveyAdminVM
    {
        public List<VBSParticipationSurveyAdminRow> Rows { get; set; } = new();
        public List<VBSParticipationSurveyDaySummary> Summaries { get; set; } = new();
        public DateTime[] Days { get; set; } = Array.Empty<DateTime>();
        // ✅ 추가: RoleCode(T1001...) -> displayOrder
        public Dictionary<string, int> RoleDisplayOrder { get; set; }
            = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    }

    public class VBSParticipationSurveyAdminRow
    {
        public string UserName { get; set; } = "";
        public string FullName { get; set; } = "";
        public string RoleCode { get; set; } = "";
        public int? ClassYear { get; set; }
        public int? ClassBan { get; set; }
        public string RoleNM { get; set; } = "";
        public Dictionary<(DateTime Date, string Slot), bool?> Slots { get; set; } = new();
        public Dictionary<(DateTime Date, string Slot), string?> StatusNotes { get; set; } = new();
    }

    public class VBSParticipationSurveyDaySummary
    {
        public DateTime Date { get; set; }
        public int AmAttend { get; set; }
        public int PmAttend { get; set; }
    }
}
