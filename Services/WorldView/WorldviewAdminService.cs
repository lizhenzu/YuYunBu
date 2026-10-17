using BulletinBoard.ViewModels.Worldview;
using System.Collections.Generic;

namespace BulletinBoard.Services.WorldView.Admin
{
    public class WorldviewAdminService : IWorldviewAdminService
    {
        private readonly WorldviewAdminRepository _repo;

        public WorldviewAdminService(WorldviewAdminRepository repo)
        {
            _repo = repo;
        }

        public List<HeaderTitleCardRow> GetHeaderTitleCards()
            => _repo.GetHeaderTitleCards();

        public int GetActiveLessonNo()
            => _repo.GetActiveLessonNo();

        public void SetActiveLessonNo(int lessonNo, string userName)
            => _repo.SetActiveLessonNo(lessonNo, userName);
    }
}
