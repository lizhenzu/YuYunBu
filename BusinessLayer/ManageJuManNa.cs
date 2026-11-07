using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace BulletinBoard.BusinessLayer
{
    public class ManageJuManNa
    {

        public List<JuManNaYYYYMM> getJuManNaGroupByYYYYMM()
        {
            ControlJuManNa controlJuManNa = new ControlJuManNa();
            DataSet ds = controlJuManNa.fetchJuManNaGroupByYYYYMM();
            List<JuManNaYYYYMM> value = ds.Tables[0].ToList<JuManNaYYYYMM>();
            return value;
        }
        /// <summary>
        /// 말씀 정리 화면(달력형) — 한 달의 날짜별 내용과 빠진 곳.
        /// 영상은 그날 말씀이 걸친 장(최대 5)마다 영상 주소가 있어야 "있음"으로 본다 (영상 구간 편집과 같은 규칙)
        /// </summary>
        public JuManNaMonthBoard getJuManNaBoard(DateTime firstDay)
        {
            var board = new JuManNaMonthBoard { First = firstDay, YYYYMM = firstDay.ToString("yyyy-MM") };

            foreach (var g in getJuManNaByYYYYMM(board.YYYYMM).GroupBy(x => x.ExposeDate.Date).OrderBy(g => g.Key))
            {
                board.Days.Add(ToBoardDay(g.First()));
            }
            return board;
        }

        /// <summary>
        /// 하루치를 화면용으로 바꾼다.  입력 여부 네 가지 (빠진 것은 Missing 에 이름이 들어간다):
        ///  · 키워드   = 주제(BibleSubject)가 있음
        ///  · 줄거리   = Synopsis 가 있음
        ///  · 말씀 구간 = 성경 이름 · 시작 장 · 시작 절 · 끝 장 · 끝 절이 모두 있음
        ///  · 영상 구간 = 그날 말씀이 걸친 장(최대 5)의 시작 · 종료 시점 중 하나라도 지정됨 (0 = 기본값이라 지정 안 한 것)
        /// </summary>
        public static JuManNaBoardDay ToBoardDay(JuManNas x)
        {
            var day = new JuManNaBoardDay
            {
                Id = x.Id,
                Date = x.ExposeDate.Date.ToString("yyyy-MM-dd"),
                Ref = $"{x.BibleText} {SharedComm.MakeBibleSerialJangZul(x.BibleText, x.FromJang, x.FromZul, x.ToJang, x.ToZul)}".Trim(),
                Subject = (x.BibleSubject ?? "").Trim(),
                Synopsis = (x.Synopsis ?? "").Trim(),
                Summary = (x.BibleSummary ?? "").Trim()
            };

            var urls   = new[] { x.URL, x.URL2, x.URL3, x.URL4, x.URL5 };
            var starts = new[] { x.YouTuBuStartMM * 60 + x.YouTuBuStartSS, x.YouTuBuStartMM2 * 60 + x.YouTuBuStartSS2, x.YouTuBuStartMM3 * 60 + x.YouTuBuStartSS3, x.YouTuBuStartMM4 * 60 + x.YouTuBuStartSS4, x.YouTuBuStartMM5 * 60 + x.YouTuBuStartSS5 };
            var ends   = new[] { x.YouTuBuEndMM * 60 + x.YouTuBuEndSS, x.YouTuBuEndMM2 * 60 + x.YouTuBuEndSS2, x.YouTuBuEndMM3 * 60 + x.YouTuBuEndSS3, x.YouTuBuEndMM4 * 60 + x.YouTuBuEndSS4, x.YouTuBuEndMM5 * 60 + x.YouTuBuEndSS5 };

            string F(int sec) => $"{sec / 60}:{sec % 60:D2}";
            int need = Math.Min(5, Math.Max(1, x.ToJang - x.FromJang + 1));
            bool clipSet = false;
            for (int i = 0; i < 5; i++)
            {
                if (!string.IsNullOrWhiteSpace(urls[i])) day.Clips.Add($"{F(Math.Max(0, starts[i]))} ~ {(ends[i] > 0 ? F(ends[i]) : "끝")}");
                if (i < need && (starts[i] > 0 || ends[i] > 0)) clipSet = true;
            }

            bool rangeSet = !string.IsNullOrWhiteSpace(x.BibleText) && x.FromJang >= 1 && x.FromZul >= 1 && x.ToJang >= 1 && x.ToZul >= 1;

            if (day.Subject.Length == 0) day.Missing.Add("키워드");
            if (day.Synopsis.Length == 0) day.Missing.Add("줄거리");
            if (!rangeSet) day.Missing.Add("말씀 구간");
            if (!clipSet) day.Missing.Add("영상 구간");
            return day;
        }
        public List<JuManNas> getJuManNa(int Id, bool activeData, DateTime workDT)
        {
            ControlJuManNa controlJuManNa = new ControlJuManNa();
            DataSet ds = controlJuManNa.fetchJuManNa(Id, activeData, workDT);
            List<JuManNas> value = ds.Tables[0].ToList<JuManNas>();
            return value;
        }

        public List<JuManNas> getJuManNaByYYYYMM(string YYYYMM)
        {
            ControlJuManNa controlJuManNa = new ControlJuManNa();
            DataSet ds = controlJuManNa.fetchJuManNaByYYYYMM(YYYYMM);
            List<JuManNas> value = ds.Tables[0].ToList<JuManNas>();
            return value;
        }

        public void UpdateJuManNa(JuManNa param)
        {
            ControlJuManNa controlJuManNa = new ControlJuManNa();

            controlJuManNa.UpdaetJuManNa(param);
        }


        /// <summary>월별 일괄 입력 — 그 달 1일 ~ 말일.  표에 있는 날은 값을 채우고, 없는 날은 Id 0</summary>
        public JuManNaMonthly getJuManNaMonthly(DateTime firstDay)
        {
            string ym = firstDay.ToString("yyyy-MM");
            var existing = ExistingByDate(ym);

            var result = new JuManNaMonthly { YYYYMM = ym };
            int days = DateTime.DaysInMonth(firstDay.Year, firstDay.Month);
            for (int d = 1; d <= days; d++)
            {
                DateTime dt = new DateTime(firstDay.Year, firstDay.Month, d);
                if (existing.TryGetValue(dt, out var x))
                {
                    result.Days.Add(new JuManNaMonthlyDay
                    {
                        Id = x.Id, ExposeDate = dt,
                        BibleSubject = x.BibleSubject, BibleText = x.BibleText,
                        FromJang = x.FromJang > 0 ? x.FromJang : null, FromZul = x.FromZul > 0 ? x.FromZul : null,
                        ToJang = x.ToJang > 0 ? x.ToJang : null, ToZul = x.ToZul > 0 ? x.ToZul : null
                    });
                }
                else
                {
                    result.Days.Add(new JuManNaMonthlyDay { Id = 0, ExposeDate = dt });
                }
            }
            return result;
        }

        /// <summary>
        /// 월별 일괄 저장.  화면에서 넘어온 Id 는 믿지 않고 날짜로 다시 맞춘다.
        /// 있는 날 → 바뀐 날만 수정 / 없는 날 → 새로 넣음.  반환: (수정 수, 추가 수)
        /// </summary>
        public (int updated, int inserted) SaveJuManNaMonthly(DateTime firstDay, List<JuManNaMonthlyDay> posted)
        {
            string ym = firstDay.ToString("yyyy-MM");
            var existing = ExistingByDate(ym);

            var updates = new List<JuManNaMonthlyDay>();
            var inserts = new List<JuManNaMonthlyDay>();
            foreach (var d in (posted ?? new List<JuManNaMonthlyDay>()).GroupBy(p => p.ExposeDate.Date).Select(g => g.First()))
            {
                if (d.ExposeDate.Year != firstDay.Year || d.ExposeDate.Month != firstDay.Month) continue;

                if (existing.TryGetValue(d.ExposeDate.Date, out var x))
                {
                    if (!d.Changed) continue;
                    d.Id = x.Id;
                    updates.Add(d);
                }
                else
                {
                    inserts.Add(d);
                }
            }

            new ControlJuManNa().SaveJuManNaMonthly(updates, inserts);
            return (updates.Count, inserts.Count);
        }

        private Dictionary<DateTime, JuManNas> ExistingByDate(string ym)
        {
            return getJuManNaByYYYYMM(ym)
                   .GroupBy(x => x.ExposeDate.Date)
                   .ToDictionary(g => g.Key, g => g.First());
        }

        /// <summary>영상 구간 편집 — 그 달의 날짜별 구간(장마다 한 줄)</summary>
        public JuManNaSegMonth getJuManNaSegMonth(DateTime firstDay)
        {
            DataSet ds = new ControlJuManNa().fetchJuManNaSeg(firstDay, 0);
            var result = new JuManNaSegMonth { YYYYMM = firstDay.ToString("yyyy-MM") };
            if (ds.Tables.Count > 0)
            {
                foreach (DataRow r in ds.Tables[0].Rows) result.Days.Add(ToSegDay(r));
            }
            return result;
        }

        /// <summary>하루치 영상 구간 (없으면 null) — 플랫폼으로 보낼 때 쓴다</summary>
        public JuManNaSegDay getJuManNaSegDay(int id)
        {
            DataSet ds = new ControlJuManNa().fetchJuManNaSeg(null, id);
            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0) return null;
            return ToSegDay(ds.Tables[0].Rows[0]);
        }

        /// <summary>
        /// 영상 구간 한 점 저장.  화면 값은 믿지 않고 표에서 그날을 다시 읽어 검사한다.
        /// sec 0 = 기본값으로 되돌림(시작 → 영상 처음, 종료 → 영상 끝).
        /// </summary>
        public (bool ok, string msg, JuManNaSeg seg) SaveJuManNaSegPoint(int id, int slot, bool isStart, int sec)
        {
            DataSet ds = new ControlJuManNa().fetchJuManNaSeg(null, id);
            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0) return (false, "그날을 찾지 못했어요", null);

            var seg = ToSegDay(ds.Tables[0].Rows[0]).Segs.FirstOrDefault(s => s.Slot == slot);
            if (seg == null) return (false, "이 날에 없는 구간이에요", null);
            if (sec < 0) return (false, "시간이 올바르지 않아요", null);
            if (seg.LenSec > 0 && sec > seg.LenSec) return (false, $"영상 길이({Fmt(seg.LenSec)})를 넘었어요", null);

            int end = seg.EndSec > 0 ? seg.EndSec : seg.LenSec;
            if (isStart && sec > 0 && end > 0 && sec >= end) return (false, $"시작은 종료({Fmt(end)})보다 앞이어야 해요", null);
            if (!isStart && sec > 0 && sec <= seg.StartSec) return (false, $"종료는 시작({Fmt(seg.StartSec)})보다 뒤여야 해요", null);

            new ControlJuManNa().SaveJuManNaSegPoint(id, slot, isStart, sec / 60, sec % 60);
            if (isStart) seg.StartSec = sec; else seg.EndSec = sec;
            return (true, "", seg);
        }

        private static string Fmt(int sec) => $"{sec / 60}:{sec % 60:D2}";

        private static JuManNaSegDay ToSegDay(DataRow r)
        {
            int I(string c) => r[c] == DBNull.Value ? 0 : Convert.ToInt32(r[c]);

            var day = new JuManNaSegDay
            {
                Id = I("Id"),
                ExposeDate = Convert.ToDateTime(r["ExposeDate"]),
                BibleText = r["BibleText"] as string ?? "",
                FromJang = I("FromJang"), FromZul = I("FromZul"), ToJang = I("ToJang"), ToZul = I("ToZul")
            };

            // 뷰와 같은 규칙: 구간 n = FromJang + (n-1) 장, ToJang 까지 · 최대 5장
            int count = Math.Min(5, Math.Max(1, day.ToJang - day.FromJang + 1));
            for (int n = 1; n <= count; n++)
            {
                day.Segs.Add(new JuManNaSeg
                {
                    Slot = n,
                    Jang = day.FromJang + n - 1,
                    Url = r["URL" + n] as string ?? "",
                    LenSec = I("TM" + n) * 60 + I("TS" + n),
                    StartSec = I("SM" + n) * 60 + I("SS" + n),
                    EndSec = I("EM" + n) * 60 + I("ES" + n)
                });
            }
            return day;
        }

        public void HitJuManNa(int Id)
        {
            ControlJuManNa controlJuManNa = new ControlJuManNa();
            controlJuManNa.HitJuManNa(Id);
        }
    }
}
