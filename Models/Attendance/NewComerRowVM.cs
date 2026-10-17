namespace BulletinBoard.ViewModels.Attendance
{
    public class NewComerRowVM
    {
        public int Id { get; set; }
        public int PersonId { get; set; }

        public string ChildName { get; set; } = string.Empty;
        public DateTime FirstVisitDate { get; set; }
        public string ParentName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? School { get; set; }

        public int? BirthMonth { get; set; }
        public int? BirthDay { get; set; }

        public int? ClassYear { get; set; }
        public int? ClassBan { get; set; }

        public int VisitCount { get; set; }
        public DateTime? LastVisitDate { get; set; }
        public string NewComerStatus { get; set; } = string.Empty;

        public string? PhotoPath { get; set; }

        /// <summary>자진출석 여부</summary>
        public bool IsVoluntary { get; set; } = true;

        /// <summary>전도자 이름</summary>
        public string? EvangelistName { get; set; }

        public string PhotoUrl =>
            string.IsNullOrWhiteSpace(PhotoPath)
                ? "/images/icon/people/people_01.webp"
                : PhotoPath!;

        public string BirthText =>
            BirthMonth.HasValue && BirthDay.HasValue
                ? $"{BirthMonth:00}/{BirthDay:00}"
                : "-";

        public string ClassText =>
            ClassYear.HasValue && ClassBan.HasValue
                ? $"{ClassYear}학년 {ClassBan}반"
                : "-";

        /// <summary>자진출석 / 전도자 표시 텍스트</summary>
        public string VoluntaryText =>
            IsVoluntary
                ? "자진출석"
                : (!string.IsNullOrWhiteSpace(EvangelistName) ? $"전도: {EvangelistName}" : "전도");
    }
}