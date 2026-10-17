using BulletinBoard.Models;
using System;

namespace BulletinBoard.Models.DailyMission2
{
    /// <summary>
    /// ✅ Recite 화면에서 사용할 주간 암송/공과 상세 묶음
    /// </summary>
    public class WeeklyWordDetailResult
    {
        // ✅ 주간 데이터 Row (기존 프로젝트 타입)
        public WeeklyWordToReads? WeeklyWordToReadData { get; set; }

        // ✅ 암송 구절 본문(eBibles)
        public eBibles? WeekeBiblesDataToRecite { get; set; }

        // ✅ (추가) 읽기 본문 1
        // - DailyMission2Service.GetWeeklyWordDetailAsync에서 WeekeBiblesDataToRead를 채우고 있으므로
        //   모델에도 존재해야 ViewBag.WeekeBiblesDataToRead 등으로 쓸 수 있습니다.
        public eBibles? WeekeBiblesDataToRead { get; set; }

        // ✅ (추가) 읽기 본문 2 (BibleTextToRead_1 세트)
        public eBibles? WeekeBiblesDataToRead_1 { get; set; }

        // ✅ (추가) 상태/표시용: 읽기 본문이 있는지
        public bool HasRead1 => WeekeBiblesDataToRead?.eBibleDatas != null && WeekeBiblesDataToRead.eBibleDatas.Count > 0;
        public bool HasRead2 => WeekeBiblesDataToRead_1?.eBibleDatas != null && WeekeBiblesDataToRead_1.eBibleDatas.Count > 0;

        // ✅ 기본
        public bool HasData => WeeklyWordToReadData != null;
    }
}
