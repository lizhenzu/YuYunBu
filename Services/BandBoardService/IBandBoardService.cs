// Services/IBandBoardService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using BulletinBoard.Models;
using BulletinBoard.ViewModels;

namespace BulletinBoard.Services
{
  

    public interface IBandBoardService
    {
        /// <summary>
        /// 게시판별(테이블별) 페이지 단위 조회
        /// boardTableName 예시:
        ///  - BandPosts
        ///  - BandJuboPosts
        ///  - BandAPosts
        /// </summary>
        Task<PagedPostsResult> GetPostsPageAsync(string boardTableName, int page, int pageSize);

        /// <summary>
        /// 게시판별 글 + 이미지 등록
        /// </summary>
        Task<int> CreatePostAsync(string boardTableName, string writerName, string content, IEnumerable<string> imagePaths);

        /// <summary>
        /// 게시판별 좋아요 증가
        /// </summary>
        Task<int> IncrementLikeAsync(string boardTableName, int postId);

        /// <summary>
        /// 게시판별 댓글 등록
        /// </summary>
        Task<BandPostComment> AddCommentAsync(string boardTableName, int postId, string writerName, string content);

        /// <summary>
        /// 게시판별 글 삭제 (이미지/댓글 포함)
        /// </summary>
        Task DeletePostAsync(string boardTableName, int postId);

        /// <summary>
        /// 전체 글 수 / 오늘 글 수 반환
        /// </summary>
        Task<(int Total, int Today)> GetPostCountsAsync(string boardTableName);
    }
}
