// 경로: /ViewModels/Notice/NoticeEditVm.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.ViewModels
{
    public class BirthConfirmVM
    {
        public string UserId { get; set; } = "";

        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public string? ProfilePicturePath { get; set; }

        public int BMonth { get; set; }
        public int BDay { get; set; }
        public string birthDay_PublishYN { get; set; }


        // 저장 후 이동할 URL
        public string ReturnUrl { get; set; } = "";
    }

}
