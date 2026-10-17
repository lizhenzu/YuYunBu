using BulletinBoard.Helpers;
using System;
using System.Collections.Generic;

namespace BulletinBoard.Models.SuperAdmin
{
    public class AnomalyDashboardVM
    {
        public List<IdentityDupRow> IdentityDupRows { get; set; } = new();
        public List<PersonMasterDupRow> PersonMasterDupRows { get; set; } = new();

        public int IdentityDupGroupCount { get; set; }
        public int IdentityDupUserCount { get; set; }

        public int PersonMasterDupGroupCount { get; set; }
        public int PersonMasterDupRowCount { get; set; }

        public DateTime GeneratedAt { get; set; } = DateHelper.GetKoreaNow();
    }

    public class IdentityDupRow
    {
        public string FullNameKey { get; set; } = "";
        public int Cnt { get; set; }

        public string UserId { get; set; } = "";
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? InsertDT { get; set; }

        public string? LastName { get; set; }
        public string? FirstName { get; set; }
    }

    public class PersonMasterDupRow
    {
        public string FullName { get; set; } = "";
        public int Cnt { get; set; }

        public int? PersonId { get; set; }
        public string? UserId { get; set; }

        public string? PhoneNumber { get; set; }
        public string? PhoneNumber2 { get; set; }

        public int? BYear { get; set; }
        public int? BMonth { get; set; }
        public int? BDay { get; set; }

        public string? Sex { get; set; }
        public string? School { get; set; }
        public string? HomeAddress { get; set; }

        public string? UserName { get; set; }
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? Email { get; set; }

        public DateTime? InsertDT { get; set; }
        public DateTime? UpdateDT { get; set; }

        // 옵션: 데이터 상태 파악용
        public int? ReadBibleAllCount { get; set; }
        public string? PlusMins { get; set; }
        public string? BirthDayPublishYN { get; set; }
    }

}
