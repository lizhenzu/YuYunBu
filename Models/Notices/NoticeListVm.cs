using System;
using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Notice
{

    public class NoticeListVm
    {
        public IEnumerable<NoticeListItemVm> Items { get; set; }

        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public string Keyword { get; set; }
    }
}
