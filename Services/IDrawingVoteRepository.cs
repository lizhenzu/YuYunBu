using System.Collections.Generic;

public interface IDrawingVoteRepository
{
    // 전체 그림 목록 + 현재 사용자(IP)가 좋아요 눌렀는지 여부까지
    List<DrawingVoteViewModel> GetAllWithUserState(string sortBy, string voterIp);

    // 특정 그림에 대해 좋아요 토글하고 결과 리턴
    ToggleLikeResult ToggleLike(int id, string voterIp);
}
