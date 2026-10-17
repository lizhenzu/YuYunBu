using BulletinBoard.Models;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.ViewModels.Notice
{
    public class NoticeEditVm
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [StringLength(50)]
        public string AuthorName { get; set; }

        public bool IsPinned { get; set; }

        // ✅ 팝업공지 여부
        public bool IsPopup { get; set; }

        // ✅ 팝업 공지 기간(해당일자까지 팝업)
        // - null이면 "무기한" 또는 "팝업기간 미설정" 정책으로 처리 가능
        public DateTime? PopupUntil { get; set; }

        // ✅ 기존 첨부파일 목록 (Edit 화면에서 보여주고 삭제 체크용)
        public List<NoticeAttachment> Attachments { get; set; } = new List<NoticeAttachment>();
    }
}
