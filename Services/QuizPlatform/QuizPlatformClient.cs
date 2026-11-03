// Services/QuizPlatform/QuizPlatformClient.cs
using BulletinBoard.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BulletinBoard.Services.QuizPlatform
{
    /// <summary>플랫폼에 하루치 퀴즈를 보낸 결과</summary>
    public class QuizPushResult
    {
        public string Date { get; set; } = "";
        public bool Ok { get; set; }
        public bool Skipped { get; set; }        // 보내지 않음 (O/X · 설정 없음 등)
        public int StatusCode { get; set; }
        public string? Status { get; set; }      // Published / Draft
        public bool? Created { get; set; }
        public string Message { get; set; } = "";
    }

    public interface IQuizPlatformClient
    {
        /// <summary>appsettings 에 주소와 키가 모두 있는지</summary>
        bool IsConfigured { get; }
        Task<QuizPushResult> PushAsync(QuestionTypeData quiz);
    }

    /// <summary>
    /// "도전!!! 성경말씀" 퀴즈 입력 API (PUT /api/platform/quizzes/{yyyy-MM-dd}) 호출
    /// - 설정: appsettings.json 의 QuizPlatform:BaseUrl, QuizPlatform:ApiKey (키는 Git 에 넣지 않는다)
    /// - 객관식(o) → Choice, 주관식(s) → Text, O/X(a) 는 보내지 않는다. 항상 바로 공개(publish = true)
    /// </summary>
    public class QuizPlatformClient : IQuizPlatformClient
    {
        // 비어 있는 선택 칸(background · explanation)은 아예 보내지 않는다
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping   // 한글을 그대로
        };

        private readonly HttpClient _http;
        private readonly string? _baseUrl;
        private readonly string? _apiKey;

        public QuizPlatformClient(HttpClient http, IConfiguration configuration)
        {
            _http    = http;
            _baseUrl = configuration["QuizPlatform:BaseUrl"]?.Trim().TrimEnd('/');
            _apiKey  = configuration["QuizPlatform:ApiKey"]?.Trim();
            _http.Timeout = TimeSpan.FromSeconds(30);
        }

        public bool IsConfigured => !string.IsNullOrEmpty(_baseUrl) && !string.IsNullOrEmpty(_apiKey);

        public async Task<QuizPushResult> PushAsync(QuestionTypeData quiz)
        {
            var date   = quiz.ExposeDate.ToString("yyyy-MM-dd");
            var result = new QuizPushResult { Date = date };

            if (!IsConfigured)
                return Skip(result, "플랫폼 주소나 API 키가 설정되지 않았어요.");

            object body;
            try
            {
                body = BuildBody(quiz);
            }
            catch (QuizSkipException ex)
            {
                return Skip(result, ex.Message);
            }

            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Put, $"{_baseUrl}/api/platform/quizzes/{date}")
                {
                    // 길이가 정해진 UTF-8 본문으로 보낸다 (조각 전송을 못 받는 서버 대비)
                    Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json")
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

                result.Message = ErrorMessage(result.StatusCode, text);
                return result;
            }
            catch (TaskCanceledException)
            {
                result.Message = "응답이 늦어 중단했어요. 잠시 뒤 다시 보내 주세요.";
                return result;
            }
            catch (HttpRequestException ex)
            {
                result.Message = $"플랫폼에 연결하지 못했어요. ({ex.Message})";
                return result;
            }
        }

        private static object BuildBody(QuestionTypeData quiz)
        {
            string type        = (quiz.QuestionType ?? "").Trim().ToLower();
            string? background = Clean(quiz.QuestionBackground, "blank");
            string? explanation = Clean(quiz.AnswerGuide, "...");

            if (string.IsNullOrWhiteSpace(quiz.QuestionQ))
                throw new QuizSkipException("문제가 비어 있어요.");

            if (type == "o")
            {
                var o = quiz as QuestionTypeDataObject
                        ?? throw new QuizSkipException("객관식 보기를 읽지 못했어요.");

                // 빈 보기는 빼고, 정답 번호는 남은 보기 기준으로 다시 센다
                var raw = new[] { o.Question1, o.Question2, o.Question3, o.Question4 };
                if (!int.TryParse(quiz.CorrectAnswer?.Trim(), out var answerNo) || answerNo < 1 || answerNo > 4
                    || string.IsNullOrWhiteSpace(raw[answerNo - 1]))
                    throw new QuizSkipException("정답 보기 번호가 올바르지 않아요.");

                var choices = new List<string>();
                int correctNumber = 0;
                for (int i = 0; i < raw.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(raw[i])) continue;
                    choices.Add(raw[i].Trim());
                    if (i == answerNo - 1) correctNumber = choices.Count;
                }

                return new
                {
                    question = quiz.QuestionQ.Trim(),
                    background,
                    format = "Choice",
                    choices,
                    correctNumber,
                    explanation,
                    publish = true
                };
            }

            if (type == "s")
            {
                if (string.IsNullOrWhiteSpace(quiz.CorrectAnswer))
                    throw new QuizSkipException("주관식 정답이 비어 있어요.");

                return new
                {
                    question = quiz.QuestionQ.Trim(),
                    background,
                    format = "Text",
                    acceptedAnswers = new[] { quiz.CorrectAnswer.Trim() },
                    explanation,
                    publish = true
                };
            }

            throw new QuizSkipException(type == "a" ? "O/X 퀴즈는 보내지 않아요." : $"알 수 없는 유형({quiz.QuestionType})이라 보내지 않아요.");
        }

        /// <summary>비었거나 자리 채움 값이면 null (보내지 않음)</summary>
        private static string? Clean(string? value, string placeholder)
        {
            var v = value?.Trim();
            return string.IsNullOrEmpty(v) || v == placeholder ? null : v;
        }

        private static string ErrorMessage(int statusCode, string text)
        {
            string? code = null, message = null, detail = null;
            if (TryParse(text, out var err))
            {
                code    = err.TryGetProperty("code", out var c) ? c.GetString() : null;
                message = err.TryGetProperty("message", out var m) ? m.GetString() : null;
                if (err.TryGetProperty("errors", out var es) && es.ValueKind == JsonValueKind.Object)
                    detail = string.Join(" / ", es.EnumerateObject().Select(p => p.Value.ToString()));
            }

            string head = (statusCode, code) switch
            {
                (400, "BadDate") => "날짜 모양이 틀렸어요.",
                (400, "BadJson") => "보낸 내용의 형식이 틀렸어요.",
                (400, "Invalid") => "입력 값을 확인해 주세요.",
                (401, _)         => "API 키가 없거나 틀렸어요.",
                (404, _)         => "플랫폼 주소가 틀렸거나 키가 설정되지 않았어요.",
                (503, _)         => "플랫폼 서버 설정이 빠져 있어요. 잠시 뒤 다시 보내 주세요.",
                _                => message ?? "보내지 못했어요."
            };
            return $"[{statusCode}] {head}" + (string.IsNullOrEmpty(detail) ? "" : $" {detail}");
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

        private static QuizPushResult Skip(QuizPushResult r, string message)
        {
            r.Skipped = true;
            r.Message = message;
            return r;
        }

        private class QuizSkipException : Exception
        {
            public QuizSkipException(string message) : base(message) { }
        }
    }
}
