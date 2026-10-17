using System;
using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Attendance
{
    /// <summary>
    /// 관리자용 월별 통계(Chart.js 데이터 소스)
    /// - 주별(해당 월의 주일) 전체 출석 인원
    /// - 월별(해당 월) 반별 총 출석 건수(주일 합산)
    /// </summary>
    public sealed class AttendanceMonthlyStatsVM
    {
        public int Year { get; set; }
        public int Month { get; set; }

        // 월 카드(1~12)
        public List<MonthCard> MonthCards { get; set; } = new();

        // 주별(해당 월 일요일별) 전체 출석 인원
        public List<WeeklyTotalRow> WeeklyTotals { get; set; } = new();

        // 월별(해당 월) 반별 총 출석 인원
        public List<ClassMonthlyTotalRow> ClassMonthlyTotals { get; set; } = new();

        // Top 3 (반별)
        public List<TopRankRow> TopRanks { get; set; } = new();

        public sealed class MonthCard
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public string Label { get; set; } = "";     // "1월", "2월"...
            public bool IsSelected { get; set; }
            public string Url { get; set; } = "";       // /Attendance/Stats?year=YYYY&month=MM
        }

        public sealed class WeeklyTotalRow
        {
            public DateTime AttendDate { get; set; }    // 주일
            public int TotalCount { get; set; }         // 전체 출석 인원
        }

        public sealed class ClassMonthlyTotalRow
        {
            public int ClassYear { get; set; }          // 학년
            public int ClassBan { get; set; }           // 반
            public string ClassName { get; set; } = ""; // "3학년 2반"
            public int TotalCount { get; set; }         // 월 누적 출석 인원
            public double Ratio { get; set; }           // 전체 대비 %
        }

        public sealed class TopRankRow
        {
            public int Rank { get; set; }               // 1~3
            public int ClassYear { get; set; }
            public int ClassBan { get; set; }
            public string ClassName { get; set; } = "";
            public int TotalCount { get; set; }
            public double Ratio { get; set; }
        }
    }
}
