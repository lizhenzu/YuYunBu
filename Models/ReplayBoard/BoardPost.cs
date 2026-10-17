using System;

namespace BulletinBoard.Models
{
    public sealed class BoardPost
    {
        public int Id { get; set; }

        public string Title { get; set; } = "";
        public string Content { get; set; } = "";

        // 화면 표시용 작성자 (스냅샷)
        public string Writer { get; set; } = "";

        // ✅ 로그인 연동: 작성자 사용자ID(없을 수도 있음: 레거시 글)
        public string? AuthorUserId { get; set; }

        // ✅ 공지 상단 고정(메타 테이블)
        public bool IsNotice { get; set; }

        // ✅ (NoticeBoard 전용) 상단 고정
        public bool IsPinned { get; set; }

        // ✅ (NoticeBoard 전용) 팝업 노출 종료 시각
        public DateTime? PopupUntil { get; set; }

        // ✅ 추천(좋아요) 수
        public int LikeCount { get; set; }

        // ✅ 내 좋아요 여부(로그인 사용자에게만)
        public bool IsLikedByMe { get; set; }

        public int ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
