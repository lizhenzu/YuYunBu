public class ToggleLikeResult
{
    // DB 처리까지 성공했는지 여부 (true면 정상 처리됨)
    public bool Success { get; set; }

    // 최종 상태: true면 현재 내가 좋아요 중, false면 취소된 상태
    public bool IsLikedNow { get; set; }

    // 현재 총 좋아요 수
    public int LikeCount { get; set; }

    // 대상 그림 PK
    public int Id { get; set; }
}
