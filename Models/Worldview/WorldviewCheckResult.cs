using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Worldview
{
    /// <summary>
    /// 빈칸 채점 결과 DTO
    /// </summary>
    public class WorldviewCheckResult
    {
        /// <summary>전체 정답 여부</summary>
        public bool IsCorrect { get; set; }

        /// <summary>그룹별 정답 여부 (WORD1, WORD2 ...)</summary>
        public Dictionary<string, bool> PerGroupCorrect { get; set; } = new();

        /// <summary>화면에 보여줄 메시지</summary>
        public string Message { get; set; } = "";
    }
}
