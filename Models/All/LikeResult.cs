using System.Collections.Generic;

public class LikeResult
{
    public bool Success { get; set; }      // true면 실제로 증가함
    public bool AlreadyVoted { get; set; } // 이미 눌렀던 사용자
    public DrawingVote UpdatedRow { get; set; }
}

