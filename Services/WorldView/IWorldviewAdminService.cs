using BulletinBoard.ViewModels.Worldview;
using System.Collections.Generic;

namespace BulletinBoard.Services.WorldView.Admin
{
    public interface IWorldviewAdminService
    {
        List<HeaderTitleCardRow> GetHeaderTitleCards();
        int GetActiveLessonNo();
        void SetActiveLessonNo(int lessonNo, string userName);
    }
}
