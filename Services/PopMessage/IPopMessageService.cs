using System.Collections.Generic;
using BulletinBoard.ViewModels.PopMessage;

namespace BulletinBoard.Services.Abstract
{
    public interface IPopMessageService
    {
        bool CanSendTo(string toUserName);
        PopMessageComposeVM GetComposeVM(string toUserName, string? returnUrl);
        void Send(string toUserName, string fromUserName, string messageText);

        // ✅ 추가(기존)
        string? GetUserNameByUserId(string userId);

        // Mailbox
        int GetInboxUnreadCount(string myUserName);

        // ✅ 추가(이번 이슈 해결 핵심: 대시보드 '총 건수' 표시용)
        int GetInboxTotalCount(string myUserName);
        int GetSentTotalCount(string myUserName);

        List<PopMessageListItemVM> GetInbox(string myUserName, int page, int pageSize);
        List<PopMessageListItemVM> GetSent(string myUserName, int page, int pageSize);

        // actions
        int MarkReadBulk(string myUserName, int[] ids);
        int DeleteMessage(string myUserName, int id);

        // people search (UserId not null인 사람만)
        List<PopMessagePersonVM> SearchPeople(string q, int take = 20);
        Task<int> DeleteAllByFromWhomAsync(string fromWhom);
        Task AdminBroadcastAllUsersAsync(string messageText, string fromWhom);
    }
}
