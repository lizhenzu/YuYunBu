public class DrawingVoteViewModel
{
    public int Id { get; set; }
    public int ImageNumber { get; set; }
    public string ImageUrl { get; set; }
    public string Who { get; set; }
    public int LikeCount { get; set; }

    // 현재 접속자(IP)가 이 그림에 좋아요한 상태인지
    public bool IsLikedByMe { get; set; }
}