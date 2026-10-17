// 경로: /Models/ChurchRecord/ChurchRecordHomeVM.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.ViewModels
{
    public class ChurchRecordHomeVM
    {
        public List<ClassCardVM> Classes { get; set; } = new();
        public string? SearchName { get; set; }

        // ✅ 이름 검색 결과 (전체 반 대상)
        public List<StudentHitVM> Hits { get; set; } = new();
    }

    public class StudentHitVM
    {
        public int PersonId { get; set; }
        public string FullName { get; set; } = "";
        public int ClassYear { get; set; }
        public int ClassBan { get; set; }
    }
}
