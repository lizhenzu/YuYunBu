using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models.EntranceExamPrays;
using System;
using System.Collections.Generic;
using System.Data;

namespace BulletinBoard.BusinessLayer
{
    /// <summary>
    /// 수시·수능 준비생을 위한 기도문 — 비즈니스 로직
    /// ※ ManageHealingAndRestorationPray 와 같은 구조입니다.
    /// </summary>
    public class ManageEntranceExamPray
    {
        /// <summary>
        /// 라운드로빈 기준일 — 이 날짜에 1번(SortOrder 가 가장 작은) 기도문이 보입니다.
        /// 다음 날 2번, 그 다음 날 3번 … 33번까지 간 뒤 다시 1번으로 돌아옵니다.
        ///
        /// 순서를 처음부터 다시 시작하고 싶으면 이 날짜만 바꾸면 됩니다.
        /// </summary>
        public const string BaseDate = "2026-09-06";

        /// <summary>
        /// 오늘 날짜(KST). 기도문 선택과 "N번째" 표시가 반드시 같은 시계를 쓰도록
        /// 여기 한 곳에서만 구합니다.
        /// </summary>
        private static DateTime TodayKst => Helpers.DateHelper.GetKoreaNow().Date;

        private readonly ControlEntranceExamPray _ctl = new ControlEntranceExamPray();

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

        private static EntranceExamPray Map(DataRow r) => new EntranceExamPray
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
            RotateYN   = r.Table.Columns.Contains("RotateYN") ? S(r, "RotateYN") : "Y",
            CreatedAt  = D(r, "CreatedAt"),
            UpdatedAt  = r.Table.Columns.Contains("UpdatedAt") && r["UpdatedAt"] != DBNull.Value
                            ? D(r, "UpdatedAt") : (DateTime?)null,
            RotationNo = I(r, "RotationNo")
        };

        private static List<EntranceExamPray> MapAll(DataSet ds)
        {
            var list = new List<EntranceExamPray>();
            if (ds == null || ds.Tables.Count == 0) return list;
            foreach (DataRow r in ds.Tables[0].Rows) list.Add(Map(r));
            return list;
        }

        // ── 조회 ─────────────────────────────────────

        /// <summary>목록에 보이는 기도문 건수 (UseYN='Y')</summary>
        public int GetActiveCount()
        {
            try { return _ctl.GetActiveCount(); }
            catch { return 0; }
        }

        /// <summary>
        /// 매일 순환에 들어가는 기도문 건수.
        /// "N편 중 M번째" 의 N 은 반드시 이 값이라야 합니다.
        /// 특정한 날에만 읽는 기도문(RotateYN='N')은 여기서 빠집니다.
        /// </summary>
        public int GetRotationCount()
        {
            try { return _ctl.GetRotationCount(); }
            catch { return 0; }
        }

        /// <summary>오늘의 기도문 (없으면 null)</summary>
        public EntranceExamPray GetTodayPray()
        {
            var list = MapAll(_ctl.GetTodayPray(BaseDate, TodayKst));
            return list.Count > 0 ? list[0] : null;
        }

        /// <summary>
        /// 목록 (미사용 포함, 순환 차례 순).
        /// ※ 화면 왼쪽 번호(RotationNo)가 1,2,3… 으로 이어져 보이려면
        ///    줄 순서도 반드시 같은 기준(SortOrder)이라야 합니다.
        ///    수정일 순으로 늘어놓으면 번호가 뒤죽박죽으로 보입니다.
        /// </summary>
        public List<EntranceExamPray> GetList(bool includeUnused = true)
            => MapAll(_ctl.GetList(includeUnused, orderByRecent: false));

        /// <summary>
        /// 공개 목록 — 사용중만, 순환 차례 순.
        /// 왼쪽 번호가 곧 "며칠째에 나오는지"이므로 줄 순서와 번호가 함께 갑니다.
        /// </summary>
        public List<EntranceExamPray> GetPublicList()
            => MapAll(_ctl.GetList(includeUnused: false, orderByRecent: false));

        /// <summary>단건</summary>
        public EntranceExamPray GetById(int prayId)
        {
            var list = MapAll(_ctl.GetById(prayId));
            return list.Count > 0 ? list[0] : null;
        }

        /// <summary>이 기도문이 순환 목록에서 몇 번째인지 (1부터). 숨김이면 0.</summary>
        public int GetRotationNo(int prayId)
        {
            try { return _ctl.GetRotationNo(prayId); }
            catch { return 0; }
        }

        /// <summary>
        /// 오늘이 순환 목록에서 몇 번째인지 (1부터). 건수가 0이면 0.
        /// ⚠️ rotationCount 에는 GetRotationCount() 값을 넘겨야 합니다.
        ///    목록 전체 건수(GetActiveCount)를 넘기면 화면의 순번이 실제와 어긋납니다.
        /// </summary>
        public int GetTodayRotationNo(int rotationCount)
        {
            if (rotationCount <= 0) return 0;
            var basis = DateTime.Parse(BaseDate).Date;
            int diff = (int)(TodayKst - basis).TotalDays;   // 기도문 선택과 같은 시계
            int idx = ((diff % rotationCount) + rotationCount) % rotationCount;
            return idx + 1;
        }

        // ── 변경 ─────────────────────────────────────

        public void IncreaseClick(int prayId) => _ctl.IncreaseClick(prayId);

        /// <summary>검증 완료 — 수정 시각만 지금(KST)으로 올립니다. 내용은 그대로입니다.</summary>
        public void TouchUpdatedAt(int prayId) => _ctl.TouchUpdatedAt(prayId);

        /// <summary>가장 오래 손대지 않은 기도문의 PrayId (없으면 0)</summary>
        public int GetOldestUpdatedPrayId()
        {
            try { return _ctl.GetOldestUpdatedPrayId(); }
            catch { return 0; }
        }

        /// <summary>좋아요 +1 → 올라간 뒤의 값</summary>
        public int IncreaseLike(int prayId) => _ctl.IncreaseLike(prayId);

        /// <summary>현재 좋아요 수</summary>
        public int GetLikeCount(int prayId) => _ctl.GetLikeCount(prayId);

        public void Insert(EntranceExamPray m)
            => _ctl.Insert(m.Title, m.PrayText, m.BibleRef, m.BibleText, m.SortOrder, m.UseYN, m.RotateYN);

        public void Update(EntranceExamPray m)
            => _ctl.Update(m.PrayId, m.Title, m.PrayText, m.BibleRef, m.BibleText, m.SortOrder, m.UseYN, m.RotateYN);

        public void Delete(int prayId) => _ctl.Delete(prayId);
    }
}
