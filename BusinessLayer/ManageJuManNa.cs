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

        public void HitJuManNa(int Id)
        {
            ControlJuManNa controlJuManNa = new ControlJuManNa();
            controlJuManNa.HitJuManNa(Id);
        }
    }
}
