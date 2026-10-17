using System;
using System.Collections.Generic;
using BulletinBoard.Models;
using BulletinBoard.ViewModels.Notice;

namespace BulletinBoard.Services.Abstract
{
    public interface INoticeService
    {
        NoticeListVm GetNoticeList(int page, int pageSize, string keyword);
        Notice GetNoticeAndIncreaseViewCount(int id);
        NoticeEditVm GetNoticeForEdit(int id);

        int CreateNotice(NoticeEditVm vm);
        bool UpdateNotice(NoticeEditVm vm);

        // ✅ 첨부/삭제 포함 오버로드 (컨트롤러가 쓰는 형태)
        int CreateNotice(NoticeEditVm vm, List<NoticeAttachmentCreateDto> attachments);
        bool UpdateNotice(NoticeEditVm vm, List<NoticeAttachmentCreateDto> attachments, List<long> deleteAttachmentIds);

        // ✅ 첨부 조회(다운로드)
        List<NoticeAttachment> GetAttachments(int noticeId);
        NoticeAttachment GetAttachment(long attachmentId);

        // ✅ 팝업 공지 조회
        List<Notice> GetActivePopupNotices(DateTime now, int topN = 5);

        // ✅ 배너용: 상단고정(IsPinned=1) 공지 여러 건 조회 (조회수 증가 없음)
        // - PopupUntil이 있으면 '클라이언트 오늘' 기준으로 만료된 건 제외
        // - 최대 maxN 건까지만 반환(너무 많아지는 것 방지)
        List<Notice> GetPinnedNoticesForBanner(DateTime now, int maxN = 20);

        bool SoftDeleteNotice(int id);
    }
}
