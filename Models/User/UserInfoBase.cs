using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BulletinBoard.Models
{
    public class UserInfoBase
    {
        public long sequenceNo { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }

        public string UserName { get; set; }

        [Display(Name = "생년월일의 월(Month)")]
        public int bMonth { get; set; }

        [Display(Name = "생년월일의 일(Day)")]
        public int bDay { get; set; }
        [Display(Name = "생일공개여부")]
        public string birthDay_PublishYN { get; set; }

        [Display(Name = "성(예: '홍')")]
        public string LastName { get; set; }

        [Display(Name = "이름(예: '길동')")]
        public string FirstName { get; set; }

        public string FullName { get; set; }

        [Display(Name = "핸드폰1")]
        public string PhoneNumber { get; set; }
        [Display(Name = "핸드폰2")]
        public string PhoneNumber2 { get; set; }


        public string HomeAaddress { get; set; }
        public string school { get; set; }
        public string sex { get; set; }

        public int readBibleAllCount { get; set; }

        [Display(Name = "학년")]
        public int ClassYear { get; set; }
        [Display(Name = "반")]
        public int ClassBan { get; set; }

        public string DeptCode { get; set; }
        public string RoleCode { get; set; }

        public string DeptNM { get; set; }
        public string RoleNM { get; set; }


        public string ProfilePicturePath { get; set; }

    }

    public class UserInfoBase2
    {
        public string UserName { get; set; }
        public string UserId { get; set; }

        [Display(Name = "성(예: '홍')")]
        public string LastName { get; set; }

        [Display(Name = "이름(예: '길동')")]
        public string FirstName { get; set; }

        public string FullName { get; set; }
    }
}