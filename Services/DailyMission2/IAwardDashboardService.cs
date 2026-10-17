using BulletinBoard.Models;
using BulletinBoard.Models.DailyMission2;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace BulletinBoard.Services.DailyMission2
{
   

    public interface IAwardDashboardService
    {
        Task<List<AwardDashboardRow>> GetDashboardAsync(int cYear);
        Task<List<AwardUserClaimRow>> GetDashboard2Async(int cYear);
        Task<List<AwardUserClaimRow>> GetMyAwardsAsync(string userName);
        Task MarkDeliveredAsync(string userName, int cYear, int awardLevel);

        // ✅ 추가: 포인트 도달 시 Eligible 자동 생성 + 새로 생성된 레벨 반환
        Task<List<int>> EnsureEligibleAwardsAsync(string userName, int cYear, int yearTotalPoint);
        // ✅ (대시보드 진입용) 도달한 AwardBase에 대해 AwardUserClaim(Eligible) 보장
        Task EnsureEligibleAwardsByTotalAsync(string userName, int cYear, int yearTotalPoint);

        // ✅ (대시보드 진입용) 아직 "선물 선택 안 함" 상태가 있는지 조회
        // yearTotalPoint: 실제 누적 포인트 — 이 값 이하의 AwardLevel만 팝업 대상
        Task<(bool hasPending, int pendingLevel)> HasPendingAwardSelectionAsync(string userName, int cYear, int yearTotalPoint);


        // ✅ (단계형) 다음 미선택 레벨(가장 낮은 레벨) 조회
        Task<(bool hasPending, int awardLevel, int amountOfMoney)> GetNextPendingAwardAsync(string userName, int cYear, int? startLevel = null);

        // ✅ 레벨에 해당하는 상품 리스트(가격 필터: UnitPrice < AmountOfMoney, 정렬: 이름순)
        Task<List<AwardItemDTO>> GetItemsForAwardLevelAsync(int awardLevel);

        // ✅ 선택 저장(즉시 저장)
        Task<bool> SaveGiftSelectionAsync(string userName, int cYear, int awardLevel, int productId);

        // ✅ 특정 레벨만 재선택(Delivered 잠금)
        Task<bool> ResetGiftSelectionAsync(string userName, int cYear, int awardLevel);

        // ✅ 완료 화면용: 올해 내 클레임 + 선택 상품 정보(조인)
        Task<List<AwardClaimWithItemDTO>> GetMyAwardClaimsWithItemsAsync(string userName, int cYear);

        Task<AwardPopupResult> HandleAwardIfReachedAsync(
           string userName,
           int cYear,
           int yearTotalPoint,
           int addedPoint
       );
    }
 

   
}
