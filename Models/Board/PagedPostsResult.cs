// 경로: /ViewModels/Notice/NoticeEditVm.cs
using BulletinBoard.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.ViewModels
{
    public class PagedPostsResult
    {
        public IEnumerable<BandPost> Posts { get; set; }
        public bool HasMore { get; set; }
        public int NextPage { get; set; }
    }
}
