using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Worldview
{

    public class HeaderTitleCardRow
    {
        public int ContentId { get; set; }
        public int LessonNo { get; set; }   // ✅ 과 번호
        public string SectionKey { get; set; } = "";
        public string Title { get; set; } = "";
    }

}
