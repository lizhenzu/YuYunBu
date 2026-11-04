// Services/ContentPlatform/ContentPlatformClient.cs
using BibleReadingThrough.Data;
using BulletinBoard.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BulletinBoard.Services.ContentPlatform
{
    /// <summary>플랫폼에 하루치 말씀 · 퀴즈를 보낸 결과</summary>
    public class ContentPushResult
    {
        public string Date { get; set; } = "";
        public bool Ok { get; set; }
        public bool Skipped { get; set; }        // 보내지 않음 (O/X · 칸 비었음 · 설정 없음 등)
        public bool Fatal { get; set; }          // 401 · 404 · 503 — 다음 날도 똑같이 실패하므로 한 달 보내기를 멈춘다
        public int StatusCode { get; set; }
        public string? Code { get; set; }        // BadDate · BadJson · Invalid · BadKey · NotReady
        public string? Status { get; set; }      // Published / Draft
        public bool? Created { get; set; }
        public string Message { get; set; } = "";
        public Dictionary<string, string>? Errors { get; set; }   // Invalid 일 때 칸별 안내
    }

    public interface IContentPlatformClient
    {
        /// <summary>사이트 주소(QuizPlatform:BaseUrl)와 API 키(CONTENT_API_KEY)가 모두 있는지</summary>
        bool IsConfigured { get; }
        Task<ContentPushResult> PushQuizAsync(QuestionTypeData quiz);
        Task<ContentPushResult> PushWordAsync(TodayWordToRead word);
    }

    /// <summary>
    /// "도전!!! 성경말씀" 콘텐츠 입력 API
    /// - 말씀: PUT /api/platform/words/{yyyy-MM-dd},  퀴즈: PUT /api/platform/quizzes/{yyyy-MM-dd}
    /// - 키: 환경 변수 CONTENT_API_KEY (설정에서 같은 이름으로 읽는다. 코드 · Git · 로그에 쓰지 않는다)
    /// - 사이트 주소: appsettings 의 QuizPlatform:BaseUrl
    /// - 항상 바로 공개(publish = true), 같은 날은 확인 없이 덮어쓴다
    /// - 400 · 401 · 404 · 503 은 다시 부르지 않는다. 500 대 · 네트워크 오류만 잠시 뒤 두 번까지 다시 부른다
    /// </summary>
    public class ContentPlatformClient : IContentPlatformClient
    {
        // 비어 있는 선택 칸(background · explanation · commentary)은 아예 보내지 않고, 한글은 그대로 쓴다
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private static readonly TimeSpan[] RetryDelays = { TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5) };

        private readonly HttpClient _http;
        private readonly AppDbContext _db;
        private readonly string? _baseUrl;
        private readonly string? _apiKey;

        public ContentPlatformClient(HttpClient http, IConfiguration configuration, AppDbContext db)
        {
            _http    = http;
            _db      = db;
            _baseUrl = configuration["QuizPlatform:BaseUrl"]?.Trim().TrimEnd('/');
            _apiKey  = configuration["CONTENT_API_KEY"]?.Trim();
            _http.Timeout = TimeSpan.FromSeconds(30);
        }

        public bool IsConfigured => !string.IsNullOrEmpty(_baseUrl) && !string.IsNullOrEmpty(_apiKey);

        // ─────────────────────────── 퀴즈 ───────────────────────────

        public Task<ContentPushResult> PushQuizAsync(QuestionTypeData quiz)
        {
            var date = quiz.ExposeDate.ToString("yyyy-MM-dd");
            object body;
            try
            {
                body = BuildQuizBody(quiz);
            }
            catch (SkipException ex)
            {
                return Task.FromResult(Skip(date, ex.Message));
            }
            return SendAsync("quizzes", date, body);
        }

        private static object BuildQuizBody(QuestionTypeData quiz)
        {
            string type         = (quiz.QuestionType ?? "").Trim().ToLower();
            string question     = StripQuestionPrefix(quiz.QuestionQ);
            string? background  = Clean(quiz.QuestionBackground, "blank");
            string? explanation = Clean(quiz.AnswerGuide, "...");

            if (question.Length == 0)
                throw new SkipException("문제가 비어 있어요.");

            if (type == "o")
            {
                var o = quiz as QuestionTypeDataObject
                        ?? throw new SkipException("객관식 보기를 읽지 못했어요.");

                // 빈 보기는 빼고, 정답 번호는 남은 보기 기준으로 다시 센다
                var raw = new[] { o.Question1, o.Question2, o.Question3, o.Question4 };
                if (!int.TryParse(quiz.CorrectAnswer?.Trim(), out var answerNo) || answerNo < 1 || answerNo > 4
                    || string.IsNullOrWhiteSpace(raw[answerNo - 1]))
                    throw new SkipException("정답 보기 번호가 올바르지 않아요.");

                var choices = new List<string>();
                int correctNumber = 0;
                for (int i = 0; i < raw.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(raw[i])) continue;
                    choices.Add(raw[i].Trim());
                    if (i == answerNo - 1) correctNumber = choices.Count;
                }

                return new { question, background, format = "Choice", choices, correctNumber, explanation, publish = true };
            }

            if (type == "s")
            {
                if (string.IsNullOrWhiteSpace(quiz.CorrectAnswer))
                    throw new SkipException("주관식 정답이 비어 있어요.");

                return new { question, background, format = "Text", acceptedAnswers = new[] { quiz.CorrectAnswer.Trim() }, explanation, publish = true };
            }

            throw new SkipException(type == "a" ? "O/X 퀴즈는 보내지 않아요." : $"알 수 없는 유형({quiz.QuestionType})이라 보내지 않아요.");
        }

        /// <summary>
        /// 명세: 문제 앞에 "Q." 를 붙이지 않는다 (플랫폼 화면이 붙인다).
        /// 맨 앞의 "Q." · "Q:" · "Q1." · "Q)" 만 뗀다. 문제가 생기면 이 함수만 바꾸면 된다
        /// </summary>
        private static string StripQuestionPrefix(string? question)
        {
            var q = (question ?? "").Trim();
            return Regex.Replace(q, @"^[Qq](\s*\d{1,3})?\s*[.:．)]\s*", "").Trim();
        }

        // ─────────────────────────── 말씀 ───────────────────────────

        public async Task<ContentPushResult> PushWordAsync(TodayWordToRead word)
        {
            var date = word.ExposeDate.ToString("yyyy-MM-dd");

            string title   = (word.BibleSubject ?? "").Trim();
            string summary = (word.BibleSummary ?? "").Trim();
            if (title.Length == 0)   return Skip(date, "말씀 주제(제목)가 비어 있어요.");
            if (summary.Length == 0) return Skip(date, "말씀 요약이 비어 있어요.");

            int book = BibleBooks.ToNumber(word.BibleText);
            if (book == 0) return Skip(date, $"책 이름 '{word.BibleText}' 을(를) 66권에서 찾지 못했어요.");

            int sc = word.FromJang, sv = word.FromZul, ec = word.ToJang, ev = word.ToZul;
            if (sc < 1 || sv < 1 || ec < 1 || ev < 1)
                return Skip(date, $"장 · 절이 비어 있어요. ({sc}:{sv} ~ {ec}:{ev})");
            if (ec < sc || (ec == sc && ev < sv))
                return Skip(date, $"끝 장절이 시작보다 앞이에요. ({sc}:{sv} ~ {ec}:{ev})");

            // 명세: 서버는 절이 그 장에 실제로 있는지 검사하지 않는다 → 개역 eBible 로 먼저 확인
            if (!await VerseExistsAsync(book, sc, sv)) return Skip(date, $"{word.BibleText} {sc}장 {sv}절이 성경에 없어요.");
            if (!await VerseExistsAsync(book, ec, ev)) return Skip(date, $"{word.BibleText} {ec}장 {ev}절이 성경에 없어요.");

            var body = new
            {
                title,
                book,
                startChapter = sc,
                startVerse   = sv,
                endChapter   = ec,
                endVerse     = ev,
                summary,
                commentary   = Clean(word.Synopsis, ""),
                publish      = true
            };
            return await SendAsync("words", date, body);
        }

        private async Task<bool> VerseExistsAsync(int book, int chapter, int verse)
        {
            // 책 이름 표기가 여럿일 수 있어(요한1서 · 요한일서) 그 절이 있는 책 이름들을 받아 번호로 비교한다
            var names = await _db.EBibles.AsNoTracking()
                .Where(e => e.Jang == chapter && e.Zul == verse)
                .Select(e => e.BibleText)
                .Distinct()
                .ToListAsync();
            return names.Any(n => BibleBooks.ToNumber(n) == book);
        }

        // ─────────────────────────── 공통 ───────────────────────────

        private async Task<ContentPushResult> SendAsync(string kind, string date, object body)
        {
            var result = new ContentPushResult { Date = date };
            if (!IsConfigured)
                return Skip(date, "플랫폼 주소(QuizPlatform:BaseUrl)나 API 키(CONTENT_API_KEY)가 설정되지 않았어요.");

            string json = JsonSerializer.Serialize(body, JsonOptions);

            for (int attempt = 0; ; attempt++)
            {
                bool transient;
                try
                {
                    using var req = new HttpRequestMessage(HttpMethod.Put, $"{_baseUrl}/api/platform/{kind}/{date}")
                    {
                        // 길이가 정해진 UTF-8 본문으로 보낸다 (조각 전송을 못 받는 서버 대비)
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    };
                    req.Headers.Add("X-Api-Key", _apiKey);

                    using var res = await _http.SendAsync(req);
                    result.StatusCode = (int)res.StatusCode;
                    var text = await res.Content.ReadAsStringAsync();

                    if (res.IsSuccessStatusCode)
                    {
                        result.Ok = true;
                        if (TryParse(text, out var ok))
                        {
                            result.Status  = ok.TryGetProperty("status", out var s) ? s.GetString() : null;
                            result.Created = ok.TryGetProperty("created", out var c) && c.ValueKind is JsonValueKind.True or JsonValueKind.False ? c.GetBoolean() : null;
                        }
                        result.Message = result.Created == true ? "새로 넣었어요" : "바꿨어요";
                        return result;
                    }

                    FillError(result, text);
                    transient = result.StatusCode >= 500 && result.StatusCode != 503;
                }
                catch (TaskCanceledException)
                {
                    result.StatusCode = 0;
                    result.Message = "응답이 늦어 중단했어요.";
                    transient = true;
                }
                catch (HttpRequestException ex)
                {
                    result.StatusCode = 0;
                    result.Message = $"플랫폼에 연결하지 못했어요. ({ex.Message})";
                    transient = true;
                }

                if (!transient || attempt >= RetryDelays.Length)
                {
                    if (transient) result.Message += $" ({attempt + 1}번 시도)";
                    return result;
                }
                await Task.Delay(RetryDelays[attempt]);
            }
        }

        private static void FillError(ContentPushResult r, string text)
        {
            string? message = null;
            if (TryParse(text, out var err))
            {
                r.Code  = err.TryGetProperty("code", out var c) ? c.GetString() : null;
                message = err.TryGetProperty("message", out var m) ? m.GetString() : null;
                if (err.TryGetProperty("errors", out var es) && es.ValueKind == JsonValueKind.Object)
                    r.Errors = es.EnumerateObject().ToDictionary(p => p.Name, p => p.Value.ToString());
            }

            string head = (r.StatusCode, r.Code) switch
            {
                (400, "BadDate") => "날짜 모양이 틀렸어요.",
                (400, "BadJson") => "보낸 내용의 형식이 틀렸어요.",
                (400, "Invalid") => "입력 값을 확인해 주세요.",
                (401, _)         => "API 키가 없거나 틀렸어요. 관리자에게 알려 주세요.",
                (404, _)         => "플랫폼 주소가 틀렸거나 플랫폼에 키 설정이 없어요. 관리자에게 알려 주세요.",
                (503, _)         => "플랫폼 서버 설정이 빠져 있어요. 관리자에게 알려 주세요.",
                _                => message ?? "보내지 못했어요."
            };
            r.Fatal   = r.StatusCode is 401 or 404 or 503;
            r.Message = $"[{r.StatusCode}{(r.Code != null ? " " + r.Code : "")}] {head}"
                        + (r.Errors is { Count: > 0 } ? " " + string.Join(" / ", r.Errors.Select(e => $"{e.Key}: {e.Value}")) : "");
        }

        /// <summary>비었거나 자리 채움 값이면 null (보내지 않음)</summary>
        private static string? Clean(string? value, string placeholder)
        {
            var v = value?.Trim();
            return string.IsNullOrEmpty(v) || v == placeholder ? null : v;
        }

        private static bool TryParse(string text, out JsonElement root)
        {
            root = default;
            if (string.IsNullOrWhiteSpace(text)) return false;
            try
            {
                using var doc = JsonDocument.Parse(text);
                root = doc.RootElement.Clone();
                return root.ValueKind == JsonValueKind.Object;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private static ContentPushResult Skip(string date, string message)
            => new ContentPushResult { Date = date, Skipped = true, Message = message };

        private class SkipException : Exception
        {
            public SkipException(string message) : base(message) { }
        }
    }

    /// <summary>성경 책 이름 → 개역개정 순서 번호 (1 창세기 ~ 39 말라기, 40 마태복음 ~ 66 요한계시록)</summary>
    public static class BibleBooks
    {
        private static readonly string[] Names =
        {
            "창세기","출애굽기","레위기","민수기","신명기","여호수아","사사기","룻기","사무엘상","사무엘하",
            "열왕기상","열왕기하","역대상","역대하","에스라","느헤미야","에스더","욥기","시편","잠언",
            "전도서","아가","이사야","예레미야","예레미야애가","에스겔","다니엘","호세아","요엘","아모스",
            "오바댜","요나","미가","나훔","하박국","스바냐","학개","스가랴","말라기",
            "마태복음","마가복음","누가복음","요한복음","사도행전","로마서","고린도전서","고린도후서","갈라디아서","에베소서",
            "빌립보서","골로새서","데살로니가전서","데살로니가후서","디모데전서","디모데후서","디도서","빌레몬서","히브리서","야고보서",
            "베드로전서","베드로후서","요한일서","요한이서","요한삼서","유다서","요한계시록"
        };

        // 같은 책의 다른 표기
        private static readonly Dictionary<string, int> Aliases = new()
        {
            ["요한1서"] = 62, ["요한2서"] = 63, ["요한3서"] = 64,
            ["아가서"] = 22, ["애가"] = 25, ["시편서"] = 19, ["계시록"] = 66,
        };

        private static readonly Dictionary<string, int> Map = BuildMap();

        private static Dictionary<string, int> BuildMap()
        {
            var map = new Dictionary<string, int>();
            for (int i = 0; i < Names.Length; i++) map[Names[i]] = i + 1;
            foreach (var a in Aliases) map[a.Key] = a.Value;
            return map;
        }

        /// <summary>모르는 이름이면 0</summary>
        public static int ToNumber(string? name)
        {
            var key = Regex.Replace(name ?? "", @"\s+", "");
            return Map.TryGetValue(key, out var n) ? n : 0;
        }
    }
}
