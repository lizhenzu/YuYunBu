using System;

namespace BulletinBoard.ViewModels.Worldview.Admin
{
    public class WorldviewSubmitRankUserRowVM
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }

        public int SubmitCount { get; set; }

        public int BaseSum { get; set; }
        public int BonusSum { get; set; }
        public int TotalSum { get; set; }

        public DateTime? LastSubmittedAt { get; set; }
    }
}
