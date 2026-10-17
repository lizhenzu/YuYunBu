using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Worldview
{
    public class WorldviewLessonVM
    {
        public int LessonNo { get; set; }
        public Dictionary<string, string> ContentMap { get; set; } = new();

        // ✅ 기존 유지(서버 채점용/관리용이면 OK)
        public Dictionary<string, string> BlankAnswerMap { get; set; } = new();

        // ✅ View에서 빈칸 input 박스 수를 동적으로 만들기 위한 값
        public Dictionary<string, int> BlankGroupCounts { get; set; } = new();

        public Dictionary<string, Dictionary<int, string>> BlankInitialMap { get; set; }
    = new(StringComparer.OrdinalIgnoreCase);
    }
}
