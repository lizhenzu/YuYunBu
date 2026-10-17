using System;
using System.Threading.Tasks;
using BulletinBoard.Enums;
using BulletinBoard.Models.Gift;

namespace BulletinBoard.Services.Abstract
{
    public interface IGiftPopupStateRepo
    {
        Task<GiftPopupState?> GetAsync(string UserName);

        // (호환용) 상태를 통째로 올리는 함수 - 내부적으로 MERGE
        Task UpsertAsync(GiftPopupState state);

        // 버튼 액션(누적/다음/3일/7일/선물함가기)
        Task ApplyActionAsync(string UserName, int milestone, PopupActionType actionType);

        // 로그
        Task LogActionAsync(string UserName, int milestone, PopupActionType actionType);
    }
}
