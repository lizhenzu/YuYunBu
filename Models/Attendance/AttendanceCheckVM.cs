using System;
using System.Collections.Generic;

namespace BulletinBoard.Models
{
    public sealed class AttendanceCheckVM
    {
        public DateTime Date { get; set; }

        // 선택된 반
        public int ClassYear { get; set; }
        public int ClassBan { get; set; }

        /// <summary>
        /// 현재 선택된 반 이름(서비스에서 조회/세팅). 없으면 기본 포맷으로 표시.
        /// </summary>
        public string ClassName { get; set; } = "";

        // ✅ 내 반 여부 (Q39)
        public bool IsMyClass { get; set; }

        // ✅ 내 반 정보 (Q39)
        public int MyClassYear { get; set; }
        public int MyClassBan { get; set; }
        public string? MyClassLabel { get; set; }

        // 반 선택 드롭다운용 목록
        public List<ClassItem> Classes { get; set; } = new();

        // 학생 목록
        public List<StudentRow> Students { get; set; } = new();

        // ✅ Q40-2(2차): 오늘(기준 주일) 아직 미체크인 반(0건) 안내
        // - 체크 화면 상단에서 "아직 미체크인 반 N개" 표시용
        public int NotCheckedCount { get; set; }
        public List<ClassItem> NotCheckedClasses { get; set; } = new();

        public sealed class ClassItem
        {
            public int ClassYear { get; set; }
            public int ClassBan { get; set; }
            public string ClassName { get; set; } = "";
        }

        public sealed class StudentRow
        {
            public int PersonId { get; set; }
            public string StudentName { get; set; } = "";
            public bool IsPresent { get; set; }
        }
    }
}
