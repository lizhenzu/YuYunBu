using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Worldview
{
    public class BlankSubmitRequest
    {
        public int LessonNo { get; set; }

        // ✅ 여기!
        public Dictionary<string, string[]> Answers { get; set; } = new();
    }
}
