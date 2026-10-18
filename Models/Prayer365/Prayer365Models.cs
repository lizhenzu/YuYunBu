using System;
using System.Collections.Generic;
using System.Globalization;

namespace BulletinBoard.Models.Prayer365
{
    /// <summary>
    ///  자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 날짜(월일)마다 한 편.
    ///  연도 없이 "MMdd"(0101~1231, 0229 포함)로 저장해 해마다 같은 날 같은 기도문을 보여 준다.
    ///  입력은 B 방식 — 대화창에 올린 텍스트를 Claude 가 오타 · 이상한 문구만 고쳐 넣는다 (2026-09-27 확정).
    /// </summary>
    public static class Prayer365Config
    {
        public const string BookTitle  = "자녀를 위한 365일 축복 기도문";
        public const string Author     = "한치호";
        public const string Publisher  = "도서출판 두돌비";
        public const int    TotalDays  = 366;   // 2/29 포함

        /// <summary>
        ///  true = 관리자만 볼 수 있다 (2026-09-27 — 한치호 목사님 · 출판사 허락 전).
        ///  허락을 받으면 false 로 바꾸면 누구나 본다.
        /// </summary>
        public const bool   StaffOnly  = true;

        public static readonly int[] MonthLen = { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        public static string Md(int month, int day) => $"{month:00}{day:00}";
        public static string Md(DateTime d) => Md(d.Month, d.Day);

        public static bool TryParse(string? md, out int month, out int day)
        {
            month = day = 0;
            if (string.IsNullOrWhiteSpace(md) || md.Length != 4 || !int.TryParse(md, NumberStyles.None, CultureInfo.InvariantCulture, out _)) return false;
            month = int.Parse(md[..2], CultureInfo.InvariantCulture);
            day   = int.Parse(md[2..], CultureInfo.InvariantCulture);
            return month is >= 1 and <= 12 && day >= 1 && day <= MonthLen[month - 1];
        }

        public static string Label(string md) => TryParse(md, out var m, out var d) ? $"{m}월 {d}일" : md;

        /// <summary>1년 중 몇 번째 날인지 (2/29 포함 366일 기준, 1부터)</summary>
        public static int DayIndex(string md)
        {
            if (!TryParse(md, out var m, out var d)) return 0;
            int n = 0;
            for (int i = 0; i < m - 1; i++) n += MonthLen[i];
            return n + d;
        }

        /// <summary>앞 · 뒤 날짜 (2/29 는 윤년이 아니어도 포함해 이어 준다)</summary>
        public static string Step(string md, int delta)
        {
            if (!TryParse(md, out var m, out var d)) return md;
            var dt = new DateTime(2024, m, d).AddDays(delta);   // 2024 = 윤년 — 2/29 가 있는 달력으로 계산
            return Md(dt);
        }
    }

    /// <summary>표 한 줄 (dbo.Prayer365)</summary>
    public class Prayer365Item
    {
        public string   MonthDay    { get; set; } = "";     // "0101"
        public string   Title       { get; set; } = "";
        public string?  VerseRef    { get; set; }           // "시 18:1-2"
        public string?  VerseText   { get; set; }           // 개역한글
        public string   Body        { get; set; } = "";     // 문단은 줄바꿈으로
        public List<Prayer365Fix> Fixes { get; set; } = new();   // Claude 가 고친 곳 (원문 → 교정)
        public bool     IsReviewed  { get; set; }
        public DateTime UpdatedAt   { get; set; }
        public string?  UpdatedBy   { get; set; }
        /// <summary>이 날 기도문을 연 횟수 (관리자 보기 /Prayer365 는 세지 않는다 · 2026-10-03)</summary>
        public int      ViewCount   { get; set; }
        /// <summary>"오늘 기도 했어요" 누른 수 (2026-10-17 — 모든 사용자, 하루 한 번)</summary>
        public int      PrayCount   { get; set; }

        public string Label => Prayer365Config.Label(MonthDay);
    }

    public class Prayer365Fix
    {
        public string From { get; set; } = "";
        public string To   { get; set; } = "";
    }

    /// <summary>관리 달력판 한 칸</summary>
    public class Prayer365Status
    {
        public string MonthDay   { get; set; } = "";
        public string Title      { get; set; } = "";
        public bool   IsReviewed { get; set; }
    }

    // ── 화면용 ─────────────────────────────────────────────

    public class Prayer365PageVM
    {
        public string          MonthDay { get; set; } = "";
        public bool            IsToday  { get; set; }
        public DateTime        Today    { get; set; }
        public Prayer365Item?  Item     { get; set; }
        public bool            IsStaff  { get; set; }
        public bool            IsClosed { get; set; }   // StaffOnly 인데 관리자가 아님 — 기도문을 보여 주지 않는다
        /// <summary>홈 "자녀를 위한 365" 로 들어온 일반 보기 (?view=user) — 관리자여도 관리 버튼을 숨긴다 (2026-10-03)</summary>
        public bool            UserView { get; set; }
        /// <summary>실제로 관리자로 로그인했는지 (일반 보기여도 true) — "매일 축복 기도" 를 관리자 보기로 가는 링크로 만든다</summary>
        public bool            IsAdminUser { get; set; }
        /// <summary>로그인한 사람이 오늘(KST) 이 기도문으로 "오늘 기도 했어요" 를 눌렀는지 — 개인별 기록 Prayer365PrayLog (2026-10-17)</summary>
        public bool            PrayedToday { get; set; }
        /// <summary>로그인했는지 — 화면이 하루 한 번을 서버 기록으로 볼지, 브라우저 기억으로 볼지 정한다</summary>
        public bool            SignedIn    { get; set; }
        /// <summary>"나의 기도 기록" 통계 — 로그인한 사람에게만 (없으면 null)</summary>
        public Prayer365PrayStats? Stats   { get; set; }
        /// <summary>관리자 참여자 요약(참여자 · 오늘 기도) — SuperAdmin 역할 · Basic 아이디에게만, 화면 맨 아래 (2026-10-18 홈에서 옮김)</summary>
        public Prayer365ParticipantsVM? Participants { get; set; }
        /// <summary>오늘(KST) 조회 수 · 누적 조회 수(모든 기도문 합계) — 관리자가 연 것은 세지 않는다.  -1 = 못 읽음 (2026-10-19)</summary>
        public int TodayViews { get; set; } = -1;
        public int TotalViews { get; set; } = -1;
        /// <summary>링크에 붙일 꼬리 — 일반 보기면 "&amp;view=user"</summary>
        public string ViewQs => UserView ? "&view=user" : "";
        public string Prev => Prayer365Config.Step(MonthDay, -1);
        public string Next => Prayer365Config.Step(MonthDay, +1);
    }

    /// <summary>
    ///  "나의 기도 기록" — 로그인한 사람의 Prayer365PrayLog 로 계산한 통계 (2026-10-17).
    ///  · 기도한 날 = 하루에 한 번 (기도문이 달라도 같은 날이면 하루)
    ///  · 기도율 = 처음 기도한 날 ~ 오늘까지 지난 날 중 기도한 날.  오늘 아직 안 눌렀으면 어제까지로 센다
    ///  · 연속 기도 = 오늘(안 눌렀으면 어제)부터 거꾸로 끊기지 않고 이어진 날 수
    /// </summary>
    public class Prayer365PrayStats
    {
        public int PrayedDays  { get; set; }                       // 기도한 날 수 (= "N일째")
        public int TotalDays   => Prayer365Config.TotalDays;       // 366
        public int RemainDays  => Math.Max(0, TotalDays - PrayedDays);
        public double ProgressPct => Math.Min(100.0, PrayedDays * 100.0 / TotalDays);
        public int Streak      { get; set; }
        public int Longest     { get; set; }
        public int RatePct     { get; set; } = -1;                 // -1 = 아직 기록 없음
        public bool PrayedToday { get; set; }
        public DateTime? FirstDate { get; set; }

        // 이번 달 달력 (KST 오늘 기준)
        public int MonthYear { get; set; }
        public int MonthNo   { get; set; }
        public int MonthDays { get; set; }
        public int MonthStartDow { get; set; }                      // 1일의 요일 (일=0)
        public int Today     { get; set; }                          // 오늘의 일(day)
        public HashSet<int> MonthPrayed { get; set; } = new();
        /// <summary>기도한 날 전체 (오늘까지) — 홈 카드의 "이번 주 기도 도장"이 달이 바뀌는 주에도 맞게 쓴다</summary>
        public HashSet<DateTime> Dates { get; set; } = new();

        /// <summary>월별 기도율 (오래된 달 → 최근 달, 최대 6개)</summary>
        public List<Prayer365MonthRate> Months { get; set; } = new();

        public static Prayer365PrayStats Compute(IEnumerable<DateTime> prayDates, DateTime today)
        {
            today = today.Date;
            var set = new HashSet<DateTime>();
            foreach (var d in prayDates) if (d.Date <= today) set.Add(d.Date);

            var st = new Prayer365PrayStats
            {
                PrayedDays = set.Count,
                Dates = set,
                PrayedToday = set.Contains(today),
                MonthYear = today.Year, MonthNo = today.Month, Today = today.Day,
                MonthDays = DateTime.DaysInMonth(today.Year, today.Month),
                MonthStartDow = (int)new DateTime(today.Year, today.Month, 1).DayOfWeek,
            };
            foreach (var d in set) if (d.Year == today.Year && d.Month == today.Month) st.MonthPrayed.Add(d.Day);
            if (set.Count == 0) return st;

            var first = set.Min();
            st.FirstDate = first;
            var endDay = st.PrayedToday ? today : today.AddDays(-1);   // 지난 날 계산의 끝

            // 기도율
            int elapsed = (endDay - first).Days + 1;
            if (elapsed > 0) st.RatePct = (int)Math.Round(set.Count(d => d <= endDay) * 100.0 / elapsed);

            // 연속 기도 (오늘, 안 눌렀으면 어제부터 거꾸로)
            int streak = 0;
            for (var d = endDay; set.Contains(d); d = d.AddDays(-1)) streak++;
            st.Streak = streak;

            // 최장 연속
            int longest = 0, run = 0; DateTime? prev = null;
            foreach (var d in set.OrderBy(x => x))
            {
                run = prev.HasValue && (d - prev.Value).Days == 1 ? run + 1 : 1;
                if (run > longest) longest = run;
                prev = d;
            }
            st.Longest = longest;

            // 월별 기도율 — 처음 기도한 달부터 이번 달까지, 최근 6개
            var months = new List<Prayer365MonthRate>();
            for (var m = new DateTime(first.Year, first.Month, 1); m <= new DateTime(today.Year, today.Month, 1); m = m.AddMonths(1))
            {
                var from = m < first ? first : m;
                var lastOfMonth = m.AddMonths(1).AddDays(-1);
                var to = lastOfMonth < endDay ? lastOfMonth : endDay;
                if (to < from) continue;
                months.Add(new Prayer365MonthRate
                {
                    Month = m.Month,
                    Elapsed = (to - from).Days + 1,
                    Prayed = set.Count(d => d >= from && d <= to),
                });
            }
            st.Months = months.Count > 6 ? months.GetRange(months.Count - 6, 6) : months;
            return st;
        }
    }

    /// <summary>참여자 한 명 — "오늘 기도 했어요" 를 한 번이라도 누른 회원 (2026-10-17, 관리자 전용)</summary>
    public class Prayer365Participant
    {
        public string   UserName   { get; set; } = "";
        /// <summary>이름 (view_UserCompositeInfo_2026.FullName) — 참여자 화면에서만 채운다 (2026-10-19)</summary>
        public string?  FullName   { get; set; }
        public int      PrayedDays { get; set; }
        public int      Streak     { get; set; }
        public int      RatePct    { get; set; } = -1;
        public DateTime LastAt     { get; set; }     // 마지막으로 누른 시각 (KST)
        public int      DaysAgo    { get; set; }     // 마지막 기도가 며칠 전인지 (오늘 = 0)
    }

    /// <summary>참여자 요약 + 목록 — 홈 관리자 줄 · 참여자 화면이 같이 쓴다</summary>
    public class Prayer365ParticipantsVM
    {
        public List<Prayer365Participant> Items { get; set; } = new();
        public string Sort { get; set; } = "recent";   // recent · days · rest
        public int Total    => Items.Count;
        public int Today    => Items.Count(p => p.DaysAgo == 0);
        /// <summary>마지막 기도가 3일보다 더 전인 사람</summary>
        public int Resting  => Items.Count(p => p.DaysAgo > 3);
        public int AvgRate  { get { var r = Items.Where(p => p.RatePct >= 0).Select(p => p.RatePct).ToList(); return r.Count == 0 ? 0 : (int)Math.Round(r.Average()); } }

        public static Prayer365ParticipantsVM Build(IEnumerable<(string user, DateTime day, DateTime at)> rows, DateTime today, string sort)
        {
            today = today.Date;
            var vm = new Prayer365ParticipantsVM { Sort = sort is "days" or "rest" ? sort : "recent" };
            foreach (var g in rows.GroupBy(r => r.user))
            {
                var st = Prayer365PrayStats.Compute(g.Select(x => x.day), today);
                var last = g.Max(x => x.at);
                vm.Items.Add(new Prayer365Participant
                {
                    UserName = g.Key, PrayedDays = st.PrayedDays, Streak = st.Streak, RatePct = st.RatePct,
                    LastAt = last, DaysAgo = Math.Max(0, (today - last.Date).Days),
                });
            }
            vm.Items = vm.Sort switch
            {
                "days" => vm.Items.OrderByDescending(p => p.PrayedDays).ThenByDescending(p => p.LastAt).ToList(),
                "rest" => vm.Items.OrderByDescending(p => p.DaysAgo).ThenBy(p => p.UserName).ToList(),
                _      => vm.Items.OrderByDescending(p => p.LastAt).ToList(),
            };
            return vm;
        }
    }

    public class Prayer365MonthRate
    {
        public int Month   { get; set; }
        public int Prayed  { get; set; }
        public int Elapsed { get; set; }
        public int Pct => Elapsed <= 0 ? 0 : (int)Math.Round(Prayed * 100.0 / Elapsed);
    }

    public class Prayer365AdminVM
    {
        public Dictionary<string, Prayer365Status> Map { get; set; } = new();
        public string  Filter { get; set; } = "all";   // all · empty · check · done
        public string? Q      { get; set; }
        public List<Prayer365Status> Found { get; set; } = new();   // 검색 결과
        public string  TodayMd { get; set; } = "";
        public int Filled   => Map.Count;
        public int Reviewed { get { int n = 0; foreach (var v in Map.Values) if (v.IsReviewed) n++; return n; } }
        public int ToCheck  => Filled - Reviewed;
        public int Empty    => Prayer365Config.TotalDays - Filled;
    }

    public class Prayer365EditVM
    {
        public string         MonthDay { get; set; } = "";
        public Prayer365Item? Item     { get; set; }
        public string?        Toast    { get; set; }
        public string Prev => Prayer365Config.Step(MonthDay, -1);
        public string Next => Prayer365Config.Step(MonthDay, +1);
    }
}
