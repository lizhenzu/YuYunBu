using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Birthday
{
    public class BirthdayListPageVM
    {
        public int? Month { get; set; } // null이면 연간
        public string PageTitle { get; set; } = "";
        public List<BirthdayCardVM> Items { get; set; } = new();
        public Dictionary<int, List<BirthdayCardVM>> GroupByMonth { get; set; } = new();
    }
}
