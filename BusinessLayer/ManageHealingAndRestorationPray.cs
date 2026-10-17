using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models.HealingAndRestorationPrays;
using System;
using System.Collections.Generic;
using System.Data;

namespace BulletinBoard.BusinessLayer
{
    /// <summary>
    /// 치유와 회복을 위한 기도문 — 비즈니스 로직
    /// </summary>
    public class ManageHealingAndRestorationPray
    {
        /// <summary>
        /// 라운드로빈 기준일 — 이 날짜에 1번(SortOrder 가 가장 작은) 기도문이 보입니다.
        /// 다음 날 2번, 그 다음 날 3번 … 마지막까지 간 뒤 다시 1번으로 돌아옵니다.
        ///
        /// 순서를 처음부터 다시 시작하고 싶으면 이 날짜만 바꾸면 됩니다.
        /// </summary>
        public const string BaseDate = "2026-08-27";

        /// <summary>
        /// 오늘 날짜(KST). 기도문 선택과 "N번째" 표시가 반드시 같은 시계를 쓰도록
        /// 여기 한 곳에서만 구합니다.
        /// </summary>
        private static DateTime TodayKst => Helpers.DateHelper.GetKoreaNow().Date;

        private readonly ControlHealingAndRestorationPray _ctl = new ControlHealingAndRestorationPray();

        // ── 변환 도우미 ──────────────────────────────
        private static string S(DataRow r, string c)
        {
            if (!r.Table.Columns.Contains(c)) return "";
            var o = r[c];
            return (o == null || o == DBNull.Value) ? "" : o.ToString();
        }

        private static int I(DataRow r, string c)
        {
            if (!r.Table.Columns.Contains(c)) return 0;
            var o = r[c];
            return (o == null || o == DBNull.Value) ? 0 : (int.TryParse(o.ToString(), out var v) ? v : 0);
        }

        private static DateTime D(DataRow r, string c)
        {
            if (!r.Table.Columns.Contains(c)) return DateTime.MinValue;
            var o = r[c];
            return (o == null || o == DBNull.Value) ? DateTime.MinValue
                 : (DateTime.TryParse(o.ToString(), out var v) ? v : DateTime.MinValue);
        }

        private static HealingAndRestorationPray Map(DataRow r) => new HealingAndRestorationPray
        {
            PrayId     = I(r, "PrayId"),
            SortOrder  = I(r, "SortOrder"),
            Title      = S(r, "Title"),
            PrayText   = S(r, "PrayText"),
            BibleRef   = S(r, "BibleRef"),
            BibleText  = S(r, "BibleText"),
            ClickCount = I(r, "ClickCount"),
            LikeCount  = I(r, "LikeCount"),
            UseYN      = S(r, "UseYN"),
            CreatedAt  = D(r, "CreatedAt"),
            UpdatedAt  = r.Table.Columns.Contains("UpdatedAt") && r["UpdatedAt"] != DBNull.Value
                            ? D(r, "UpdatedAt") : (DateTime?)null,
            RotationNo = I(r, "RotationNo"),
            IsVerified = B(r, "IsVerified"),
            VerifiedDT = r.Table.Columns.Contains("VerifiedDT") && r["VerifiedDT"] != DBNull.Value
                            ? D(r, "VerifiedDT") : (DateTime?)null,
            VerifiedBy = S(r, "VerifiedBy")
        };

        private static bool B(DataRow r, string c)
        {
            if (!r.Table.Columns.Contains(c)) return false;
            var o = r[c];
            if (o == null || o == DBNull.Value) return false;
            if (o is bool b) return b;
            return o.ToString() == "1" || string.Equals(o.ToString(), "True", StringComparison.OrdinalIgnoreCase);
        }

        private static List<HealingAndRestorationPray> MapAll(DataSet ds)
        {
            var list = new List<HealingAndRestorationPray>();
            if (ds == null || ds.Tables.Count == 0) return list;
            foreach (DataRow r in ds.Tables[0].Rows) list.Add(Map(r));
            return list;
        }

        // ── 조회 ─────────────────────────────────────

        /// <summary>사용중 기도문 건수</summary>
        public int GetActiveCount()
        {
            try { return _ctl.GetActiveCount(); }
            catch { return 0; }
        }

        /// <summary>오늘의 기도문 (없으면 null)</summary>
        public HealingAndRestorationPray GetTodayPray()
        {
            var list = MapAll(_ctl.GetTodayPray(BaseDate, TodayKst));
            return list.Count > 0 ? list[0] : null;
        }

        /// <summary>목록</summary>
        public List<HealingAndRestorationPray> GetList(bool includeUnused = true)
            => MapAll(_ctl.GetList(includeUnused));

        /// <summary>단건</summary>
        public HealingAndRestorationPray GetById(int prayId)
        {
            var list = MapAll(_ctl.GetById(prayId));
            return list.Count > 0 ? list[0] : null;
        }

        /// <summary>
        /// 오늘이 순환 목록에서 몇 번째인지 (1부터). 건수가 0이면 0.
        /// 화면에 "31개 중 5번째" 로 표시하기 위한 값입니다.
        /// </summary>
        public int GetTodayRotationNo(int activeCount)
        {
            if (activeCount <= 0) return 0;
            var basis = DateTime.Parse(BaseDate).Date;
            int diff = (int)(TodayKst - basis).TotalDays;   // 기도문 선택과 같은 시계
            int idx = ((diff % activeCount) + activeCount) % activeCount;
            return idx + 1;
        }

        // ── 변경 ─────────────────────────────────────

        public void IncreaseClick(int prayId) => _ctl.IncreaseClick(prayId);

        /// <summary>좋아요 +1 → 올라간 뒤의 값</summary>
        public int IncreaseLike(int prayId) => _ctl.IncreaseLike(prayId);

        /// <summary>현재 좋아요 수</summary>
        public int GetLikeCount(int prayId) => _ctl.GetLikeCount(prayId);

        public void Insert(HealingAndRestorationPray m)
            => _ctl.Insert(m.Title, m.PrayText, m.BibleRef, m.BibleText, m.SortOrder, m.UseYN);

        public void Update(HealingAndRestorationPray m)
            => _ctl.Update(m.PrayId, m.Title, m.PrayText, m.BibleRef, m.BibleText, m.SortOrder, m.UseYN);

        public void Delete(int prayId) => _ctl.Delete(prayId);

        // ── 검증 ─────────────────────────────────────

        /// <summary>표에 검증 칸이 있는지 (SQL 실행 전에는 false — 화면에서 검증 UI 를 숨긴다)</summary>
        public bool IsVerifyReady() => _ctl.HasVerifyColumns();

        /// <summary>검증 완료 표시 — 이미 검증된 것은 그대로</summary>
        public bool Verify(int prayId, string by) => _ctl.Verify(prayId, by);

        /// <summary>검증 취소 — "검증 전" 으로 되돌림</summary>
        public bool Unverify(int prayId) => _ctl.Unverify(prayId);
    }
}
