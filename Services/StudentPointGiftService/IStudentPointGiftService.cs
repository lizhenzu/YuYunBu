using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BulletinBoard.Enums;

namespace BulletinBoard.Services.Abstract
{
    public interface IStudentPointGiftService
    {
        Task<GiftPopupEval> EvaluateAsync(string userName, int integratedTotal, GiftEvalPreload? preload = null);
        /// <summary>개인 선물 점수 단계(StudentPointMilestoneCredit 의 활성 MilestonePoint, 오름차순) — 홈 선물 타임라인의 "다음 선물"</summary>
        Task<List<int>> GetGiftMilestonePointsAsync();
        Task<GiftEvalPreload> PreloadEvalStateAsync(string userName);

        Task SavePopupActionAsync(string userName, int milestone, string action);
        // action: "later", "defer3", "defer7"
        Task HidePurchaseAsync(int orderId, string adminId);
        // ✅ (신규) 팝업 버튼 액션 저장(Controller가 Repo를 직접 호출하지 않도록)
        Task HandlePopupActionAsync(string userName, int milestone, PopupActionType actionType);

        Task<List<GiftItemVM>> GetGiftListAsync(int availableCredit);
        Task<GiftBoxVM> GetMyGiftBoxAsync(string userName, int integratedTotal);
        Task<bool> IsStudentAsync(string userName);
        Task SelectGiftAsync(string userName, int productId, int integratedTotal);
        // ✅ (신규) 내 선물함에서 현재 선택(DRAFT) 취소(완전 삭제)
        Task CancelSelectedGiftAsync(string userName, int orderId);
        Task<List<AdminPurchaseRowVM>> GetAdminPurchaseListAsync(bool includeHidden = false);
        Task CompletePurchaseAsync(int orderId, string adminId);
        /// <summary>구매 완료 되돌리기 (잘못 눌렀을 때) — 다시 DRAFT 로</summary>
        Task UndoPurchaseAsync(int orderId, string adminId);
        /// <summary>전달 완료(HIDDEN) 되돌리기 — 다시 PURCHASED 로</summary>
        Task UndoHideAsync(int orderId, string adminId);
        /// <summary>가족팀 마일스톤 선물 즉시 지급 (Status=HIDDEN)</summary>
        Task InsertTeamGiftAsync(string studentUserName, int productId, int unitPrice, string teamName, string adminId, int milestone);
        /// <summary>가족팀 선물 취소 — (UserName, MileStone) 레코드 삭제</summary>
        Task CancelTeamGiftAsync(string studentUserName, int milestone);
        /// <summary>팀 선물 정보 — UserName → (Milestone → (ProductId, ProductName))</summary>
        Task<Dictionary<string, Dictionary<int,(int ProductId, string ProductName)>>> GetTeamGiftedInfoAsync();
        /// <summary>내 가족팀 소속 학생 UserName 목록 (나 자신 포함)</summary>
        Task<HashSet<string>> GetMyFamilyStudentNamesAsync(string userName);
        /// <summary>메인 페이지용: 가족 합산 DM2 >= targetPoint 이면 팀 선물 현황 반환</summary>
        Task<FamilyTeamGiftVM?> GetFamilyTeamGiftStatusAsync(string myUserName, int year, int targetPoint);
        /// <summary>가족 합산 선물 선택 (DRAFT 저장, 최고점자 userName으로)</summary>
        Task SelectFamilyGiftAsync(string topScorerUserName, int productId, int teamTotalDm2, string teamName);
        /// <summary>가족 합산 선물 취소 (DRAFT 삭제)</summary>
        Task CancelFamilyGiftAsync(string topScorerUserName, string teamName);
        /// <summary>모든 가족합산 선물 선택 현황 (teamName → (ProductId, ProductName, Status))</summary>
        Task<Dictionary<string, (int ProductId, string ProductName, string Status)>> GetFamilyCombinedGiftStatusAllAsync();
        /// <summary>DM2 합산 >= targetPoint인 모든 가족팀 목록 (선물 미선택 포함)</summary>
        Task<List<EligibleFamilyRowVM>> GetAllEligibleFamilyTeamsAsync(int year, int targetPoint);

        // ── 마일스톤 시스템 (DB 기반 동적 설정) ──────────────────
        /// <summary>홈 표시용: 연도별 활성 마일스톤 + 팀별 달성/숨김 상태 목록</summary>
        Task<List<FamilyMilestoneDisplayVM>> GetHomepageFamilyMilestonesAsync(int year);
        /// <summary>연도별 활성 마일스톤 설정 전체 목록 (달성 여부 무관)</summary>
        Task<List<FamilyMilestoneConfigVM>> GetMilestoneConfigsAsync(int year);
        /// <summary>APPLAUSE 마일스톤 숨기기 (팀 단위)</summary>
        Task HideApplauseMilestoneAsync(int milestoneConfigId, int familyTeamId);
        /// <summary>해당 유저가 특정 가족팀 소속인지 확인</summary>
        Task<bool> IsUserInFamilyTeamAsync(string userName, int familyTeamId);
        /// <summary>AWARD 마일스톤 선물 선택 (threshold 포함)</summary>
        Task SelectFamilyGiftForMilestoneAsync(string topScorerUserName, int productId, int teamTotalDm2, string teamName, int threshold);
        /// <summary>AWARD 마일스톤 선물 취소</summary>
        Task CancelFamilyGiftForMilestoneAsync(string teamName, int threshold);
        /// <summary>가족 합산 선물 — 이 팀·이 단계에서 지금 고른 선물 (아직 안 골랐으면 null)</summary>
        Task<GiftOrderVM?> GetFamilyGiftPickAsync(string teamName, int threshold);
        /// <summary>팀 이름으로 그 가족팀 구성원인지 확인 (가족 합산 선물 권한 검사용)</summary>
        Task<bool> IsUserInFamilyTeamByNameAsync(string userName, string teamName);
        /// <summary>내가 속한 가족팀이 달성한 선물 단계 전부 (박수 단계 제외)</summary>
        Task<List<FamilyMilestoneDisplayVM>> GetMyFamilyMilestonesAsync(string userName, int year);
        /// <summary>홈 가계도용: 로그인 사용자의 가족팀 구성원 (RoleCode 포함, 점수 제한 없음)</summary>
        Task<FamilyTreeVM?> GetMyFamilyTreeAsync(string userName, int year);
    }

    public sealed class GiftEvalPreload
    {
        public int? LastSeen { get; init; }
        public DateTime? BlockUntilUtc { get; init; }
        public int SpentCreditCost { get; init; }
        public int SpentMilestone { get; init; }
        public bool IsStudent { get; init; }
    }

    public sealed class FamilyTeamGiftVM
    {
        public int FamilyTeamId { get; set; }
        public string TeamName { get; set; } = "";
        public int TeamTotalDm2 { get; set; }
        public string TopScorerUserName { get; set; } = "";
        public string TopScorerFullName { get; set; } = "";
        public string TopScorerPhoto { get; set; } = "/images/icon/people/people_01.webp";
        public List<FamilyMemberGiftRow> Members { get; set; } = new();
        // 현재 선물 선택 상태
        public string? GiftStatus { get; set; }   // null=미선택, DRAFT, PURCHASED, HIDDEN
        public int OrderId { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductDescription { get; set; }
        public bool IsSelected => !string.IsNullOrWhiteSpace(GiftStatus);
        /// <summary>지금 보여 주는 단계(1000 · 3000 …). 전달 완료된 단계는 건너뛰고 다음 단계를 담는다.</summary>
        public int Threshold { get; set; } = 1000;
        public string MemoKey => Threshold == 1000 ? $"가족합산:{TeamName}" : $"가족합산:{TeamName}:{Threshold}";
    }

    public sealed class FamilyMemberGiftRow
    {
        public string UserName { get; set; } = "";
        public string FullName { get; set; } = "";
        public int Dm2Point { get; set; }
        public string Photo { get; set; } = "/images/icon/people/people_01.webp";
        public int RelationCode { get; set; } = 9;  // 0=본인
        public string RelationLabel { get; set; } = "";
    }

    public sealed class EligibleFamilyRowVM
    {
        public int FamilyTeamId { get; set; }
        public string TeamName { get; set; } = "";
        public int TeamTotalDm2 { get; set; }
        public string RecipientFullName { get; set; } = "";
        public string RecipientPhoto { get; set; } = "/images/icon/people/people_01.webp";
        public string RecipientUserName { get; set; } = "";
        public string? GiftStatus { get; set; }   // null=미선택, DRAFT, PURCHASED, HIDDEN
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        // FullName + RelationLabel (예: 엄마/아빠/본인/교사)
        public List<(string FullName, string RelationLabel)> Members { get; set; } = new();
        public List<string> MemberFullNames => Members.Select(m => m.FullName).ToList();
    }

    public sealed class GiftSelectPageVM
    {
        public int AvailableCredit { get; set; }
        public int? CurrentDraftProductId { get; set; }
        public List<GiftSelectItemVM> Gifts { get; set; } = new();
    }

    public sealed class GiftSelectItemVM
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = "";
        public string? ProductDescription { get; set; }
        public int UnitPrice { get; set; }
        public bool IsRecommended { get; set; }   // 추천 필터용(원하면)
    }

    public sealed class GiftPopupEval
    {
        public bool Show { get; set; }
        public int Milestone { get; set; }
        public int lastSeen { get; set; }    
        public int EarnedCredit { get; set; }
        public int SpentCredit { get; set; }
        public int spentMilestone { get; set; }        
        public int AvailableCredit { get; set; }
        public bool HasAffordableGift { get; set; }
    }

    public sealed class GiftItemVM
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = "";
        public string? ProductDescription { get; set; }
        public int UnitPrice { get; set; }
        public string ImageUrl => $"/images/AwardItem/{ProductID}.jpg";
    }

    public sealed class GiftBoxVM
    {
        public int IntegratedTotal { get; set; }
        public int Milestone { get; set; }
        public int EarnedCredit { get; set; }
        public int SpentCredit { get; set; }
        public int AvailableCredit { get; set; }
        public int SpentMilestone { get; set; }        

        public GiftOrderVM? CurrentDraft { get; set; }
        public List<GiftOrderVM> History { get; set; } = new();
        public List<GiftItemVM> AffordableGifts { get; set; } = new();
    }

    public sealed class GiftOrderVM
    {
        public int OrderId { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; } = "";
        public string ProductDescription { get; set; } = "";
        public int CreditCostSnapshot { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime? PurchasedAt { get; set; }
        public string ImageUrl => $"/images/AwardItem/{ProductID}.jpg";
        public int Milestone { get; set; }        
    }

    public sealed class FamilyMilestoneConfigVM
    {
        public int    PointThreshold { get; set; }
        public string MilestoneType  { get; set; } = ""; // AWARD | APPLAUSE
        public string DisplayEmoji   { get; set; } = "";
    }

    // ── 마일스톤 표시 VM ─────────────────────────────────────────
    /// <summary>홈페이지에 표시할 가족팀 × 마일스톤 1건</summary>
    public sealed class FamilyMilestoneDisplayVM
    {
        // 마일스톤 설정
        public int    MilestoneConfigId { get; set; }
        public int    PointThreshold    { get; set; }
        public string MilestoneType     { get; set; } = ""; // 'AWARD' | 'APPLAUSE'
        public string DisplayTitle      { get; set; } = "";
        public string DisplayEmoji      { get; set; } = "";

        // 팀 정보
        public int    FamilyTeamId      { get; set; }
        public string TeamName          { get; set; } = "";
        public int    TeamTotalDm2      { get; set; }
        public string RecipientUserName { get; set; } = "";
        public string RecipientFullName { get; set; } = "";
        public string RecipientPhoto    { get; set; } = "/images/icon/people/people_01.webp";
        public string RecipientSex      { get; set; } = "";
        public List<(string FullName, string RelationLabel, int YearPoint, string Photo, string Sex)> Members { get; set; } = new();
        /// <summary>숨기기 권한 체크용 — 팀 내 모든 구성원 UserName 목록</summary>
        public HashSet<string> MemberUserNames { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        // AWARD 타입 전용
        public string? GiftStatus  { get; set; }  // null | DRAFT | PURCHASED | HIDDEN
        public int?    ProductId   { get; set; }
        public string? ProductName { get; set; }

        // APPLAUSE 타입 전용
        public bool IsHidden { get; set; }

        // 홈에 표시해야 하는가
        public bool ShouldShow => MilestoneType == "AWARD"
            ? !string.Equals(GiftStatus, "HIDDEN", StringComparison.OrdinalIgnoreCase)
            : !IsHidden;
    }

    public sealed class AdminPurchaseRowVM
    {
        public int OrderId { get; set; }
        public string UserName { get; set; } = "";
        public string? UserId { get; set; }  // ✅ 추가 (프로필 사진/점수 계산용)
        public string? UserPhotoUrl { get; set; } // ✅ 추가 (프로필 이미지 URL)
        public int IntegratedTotal { get; set; }  // ✅ 추가 (최종 도토리 포인트)
        public int BiblePoint { get; set; }       // (참고) 도전 성경말씀
        public int MissionBookPoint { get; set; } // (참고) 미션북
        public int ThanksPointTotal { get; set; } // (참고) 감사노트/말씀일기
        public string RoleCode { get; set; }
        public int? ClassYear { get; set; }
        public int? ClassBan { get; set; }
        public string? UserFullName { get; set; }   // ✅ 추가
        public int ProductID { get; set; }
        public int UnitPrice { get; set; }
        public string ProductName { get; set; } = "";
        public string ProductDescription { get; set; } = "";
        public int CreditCostSnapshot { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AttendancePoint { get; set; }
        // ✅ 추가
        public string Status { get; set; } = "";
        public DateTime? PurchasedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }  // ✅ 전달일 (2026-09-26 칸 추가 — 그 전에 전달된 선물은 null)
        public string? Memo { get; set; }          // ✅ 가족팀 선물 메모 (예: "가족팀: 김민지 가족")
        public int MileStoneVal { get; set; }       // ✅ 마일스톤 (1000/2000/3000, 개인은 0)
        public bool IsTeamGift => !string.IsNullOrWhiteSpace(Memo) && Memo.StartsWith("가족팀:");
        public bool IsFamilyCombinedGift => !string.IsNullOrWhiteSpace(Memo) && Memo.StartsWith("가족합산:");
        public List<string> FamilyMemberNames { get; set; } = new();
    }

    // ── 홈 가계도용 ──────────────────────────────────────────────
    public sealed class FamilyTreeVM
    {
        public int    FamilyTeamId { get; set; }
        public string TeamName     { get; set; } = "";
        public List<FamilyTreeMemberVM> Members { get; set; } = new();
    }

    public sealed class FamilyTreeMemberVM
    {
        public string UserName  { get; set; } = "";
        public string FullName  { get; set; } = "";
        public string Photo     { get; set; } = "/images/icon/people/people_01.webp";
        public int    RoleCode  { get; set; } = 9; // 0=학생, 1=엄마, 2=아빠, 3=할머니, 4=할아버지
        public int    RelationCode { get; set; } = 9; // 학생 간 형제: 0=본인, 1=형, 2=동생, 3=언니, 4=누나
        public int    YearPoint { get; set; }
        public DateTime? LastActionDate { get; set; }
        public bool   IsMe      { get; set; }  // 로그인한 본인 여부
        // 로그인한 사용자 관점에서 이 멤버의 관계 레이블
        public string RelLabel  { get; set; } = "";
    }
}
