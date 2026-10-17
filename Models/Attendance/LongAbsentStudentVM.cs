using BulletinBoard.Models;
using System;
using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Attendance
{
    public class LongAbsentStudentVM
    {
        public UserInfo User { get; set; } = default!;

        public int AttendCount  { get; set; }   // 올해 출석 횟수
        public int WeeksCount   { get; set; }   // 올해 총 주일 수
        public int AttendRate   { get; set; }   // 출석률 (0~100)

        public DateTime? LastAttendDate { get; set; }   // 마지막 출석일
        public int DaysAbsent   { get; set; }           // 오늘 기준 결석 일수

        public string? Memo     { get; set; }   // BirthdayGiftPickMeMo (관리자)
    }

    public class LongAbsentListVM
    {
        public int AbsentDays   { get; set; } = 90;    // 기준 일수
        public int WeeksCount   { get; set; }
        public List<LongAbsentStudentVM> Students { get; set; } = new();
    }
}
