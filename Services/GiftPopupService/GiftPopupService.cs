using BulletinBoard.Models.Gift;
using BulletinBoard.Services.Abstract;
using BulletinBoard.ViewModels.Gift;

namespace BulletinBoard.Services
{
    public interface IGiftPopupService
    {
        Task<GiftPopupEvalResult> EvaluateAsync(string userName, int integratedTotal);
    }

    public class GiftPopupService : IGiftPopupService
    {
        private readonly IGiftPopupStateRepo _stateRepo;
        // TODO: credit 계산 repo/service 주입 (마일스톤 credit + 사용/소멸 credit)
        public GiftPopupService(IGiftPopupStateRepo stateRepo)
        {
            _stateRepo = stateRepo;
        }

        public async Task<GiftPopupEvalResult> EvaluateAsync(string userName, int integratedTotal)
        {
            var nowUtc = DateTime.UtcNow;
            var currentMilestone = GiftMilestone.GetCurrentMilestone(integratedTotal);

            var state = await _stateRepo.GetAsync(userName) ?? new GiftPopupState
            {
                UserName = userName,
                LastSeenMilestone = 0,
                BlockUntilUtc = null,
                UpdatedAtUtc = nowUtc
            };

            var blocked = state.BlockUntilUtc.HasValue && state.BlockUntilUtc.Value > nowUtc;

            var show =
                currentMilestone >= GiftMilestone.Min &&
                currentMilestone <= GiftMilestone.Max &&
                !blocked &&
                currentMilestone > state.LastSeenMilestone;

            return new GiftPopupEvalResult
            {
                ShowGiftPopup = show,
                PopupMilestone = show ? currentMilestone : 0,
                BlockUntilUtc = state.BlockUntilUtc,

                // TODO: credit 계산 연결
                EarnedCredit = 0,
                SpentCredit = 0
            };
        }
    }
}
