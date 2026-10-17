using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class PagedResult<T>
    {
        public IList<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public PagedResult()
        {
            Items = new List<T>();
        }
    }
}
