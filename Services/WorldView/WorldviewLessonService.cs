using BulletinBoard.DataAccessLayer.Worldview;
using BulletinBoard.Services.Abstract;
using BulletinBoard.ViewModels.Worldview;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;          // ✅ EnumerateRunes
using System.Text.Json;
using System.Threading.Tasks;

namespace BulletinBoard.Services
{
    public class WorldviewLessonService : IWorldviewLessonService
    {
        private readonly WorldviewLessonRepository _repo;

        public WorldviewLessonService(WorldviewLessonRepository repo)
        {
            _repo = repo;
        }

        public Task<Dictionary<int, LessonDashStatusVM>> GetDashboardStatusMapAsync(string? userId, string? userName)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Task.FromResult(new Dictionary<int, LessonDashStatusVM>());

            var map = _repo.GetDashboardStatusMap(userId);
            return Task.FromResult(map ?? new Dictionary<int, LessonDashStatusVM>());
        }

        public Task<WorldviewLessonVM> GetLessonAsync(int lessonNo)
        {
            if (lessonNo <= 0) lessonNo = 1;

            var contentMap = _repo.GetContentMap(lessonNo) ?? new Dictionary<string, string>();

            Dictionary<string, Dictionary<int, string>> initialMap;
            var correctMap = _repo.GetBlankCharAnswerMap(lessonNo, out initialMap)
                           ?? new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            initialMap ??= new Dictionary<string, Dictionary<int, string>>(StringComparer.OrdinalIgnoreCase);

            var vm = new WorldviewLessonVM
            {
                LessonNo = lessonNo,
                ContentMap = contentMap,

                // ✅ 정답 미노출
                BlankAnswerMap = new Dictionary<string, string>(),

                // ✅ 그룹별 글자칸 수
                BlankGroupCounts = correctMap.ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value?.Count ?? 0,
                    StringComparer.OrdinalIgnoreCase
                ),

                // ✅ placeholder용(초성 맵)
                BlankInitialMap = initialMap
            };

            return Task.FromResult(vm);
        }

        /// <summary>
        /// ✅ "제출하기(채점)" 버튼: 채점 + Draft(초안) Upsert(BonusPoint/IsAllCorrect만)
        /// </summary>
        public Task<WorldviewCheckResult> CheckBlanksAsync(
            int lessonNo,
            Dictionary<string, string[]> answers,
            string? userId = null,
            string? userName = null)
        {
            if (lessonNo <= 0)
                throw new ArgumentException("lessonNo must be > 0", nameof(lessonNo));

            answers ??= new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

            Dictionary<string, Dictionary<int, string>> initialMap;
            var correctMap = _repo.GetBlankCharAnswerMap(lessonNo, out initialMap)
                           ?? new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            initialMap ??= new Dictionary<string, Dictionary<int, string>>(StringComparer.OrdinalIgnoreCase);

            if (correctMap.Count == 0)
            {
                return Task.FromResult(new WorldviewCheckResult
                {
                    IsCorrect = false,
                    Message = "정답 데이터가 아직 등록되지 않았어요.",
                    PerGroupCorrect = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
                });
            }

            var result = new WorldviewCheckResult
            {
                PerGroupCorrect = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
            };

            foreach (var kv in correctMap)
            {
                var groupKey = kv.Key;

                var correctChars = (kv.Value ?? new List<string>())
                    .Select(NormalizeOneChar)
                    .ToArray();

                var userChars = answers.TryGetValue(groupKey, out var u) && u != null
                    ? u.Select(NormalizeOneChar).ToArray()
                    : Array.Empty<string>();

                bool ok = userChars.Length == correctChars.Length;

                if (ok)
                {
                    for (int i = 0; i < correctChars.Length; i++)
                    {
                        if (!string.Equals(userChars[i], correctChars[i], StringComparison.Ordinal))
                        {
                            ok = false;
                            break;
                        }
                    }
                }

                result.PerGroupCorrect[groupKey] = ok;
            }

            result.IsCorrect = result.PerGroupCorrect.Values.All(x => x);
            result.Message = result.IsCorrect ? "정답이에요! 🎉" : "아직 틀린 빈칸이 있어요. 다시 한 번 도전!";

            // ✅ 채점 시 Draft 레코드 Upsert (BonusPoint/IsAllCorrect만 최신화)
            if (!string.IsNullOrWhiteSpace(userId))
            {
                try
                {
                    int bonusPoint = result.IsCorrect ? 2 : 0;
                    _repo.UpsertDraftSubmitBonus(
                        lessonNo: lessonNo,
                        userId: userId!,
                        userName: userName,          // ✅ userName까지 저장
                        isAllCorrect: result.IsCorrect,
                        bonusPoint: bonusPoint
                    );
                }
                catch
                {
                    // Draft 저장 실패는 채점 UX를 막지 않음
                }
            }

            // ✅ 기존: 학습 로그 저장(빈칸 제출 로그)
            try
            {
                var json = JsonSerializer.Serialize(answers);
                _repo.InsertBlankSubmitChar(lessonNo, userId, json, result.IsCorrect);
            }
            catch
            {
                // ignore
            }

            return Task.FromResult(result);
        }

        /// <summary>
        /// ✅ "미션 제출" 버튼: (항상) 서버에서 재채점 → Draft 최신화 → Finalize(Update)로 완성
        /// </summary>
        public async Task<LessonSubmitResult> SubmitLessonAsync(
            int lessonNo,
            Dictionary<string, string[]> answers,
            string? userId,
            string? userName)
        {
            if (lessonNo <= 0) lessonNo = 1;
            answers ??= new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

            // 로그인 없이 점수 적립 금지 정책 유지
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new LessonSubmitResult
                {
                    Ok = false,
                    AlreadySubmitted = false,
                    Message = "로그인 후 제출할 수 있어요."
                };
            }

            // 정답으로 이미 완료한 경우만 차단 (오답 제출은 재도전 허용)
            if (_repo.ExistsSubmit(lessonNo, userId))
            {
                return new LessonSubmitResult
                {
                    Ok = true,
                    AlreadySubmitted = true,
                    IsAllCorrect = true,
                    BasePoint = 0, BonusPoint = 0, TotalPoint = 0,
                    Message = "이미 정답으로 완료한 레슨이에요! 🎉"
                };
            }

            const int basePoint = 3;

            // 서버에서 재채점 + Draft 최신화
            var check = await CheckBlanksAsync(lessonNo, answers, userId, userName);

            // ── 케이스 1: 이전에 오답으로 제출했던 기록이 있음 ──
            if (_repo.ExistsWrongSubmit(lessonNo, userId))
            {
                if (check.IsCorrect)
                {
                    // 정답 달성 → 기존 오답 레코드를 정답으로 업그레이드
                    const int bonus = 2;
                    _repo.UpgradeToCorrect(lessonNo, userId, userName, bonus);
                    return new LessonSubmitResult
                    {
                        Ok = true, AlreadySubmitted = false, IsAllCorrect = true,
                        BasePoint = basePoint, BonusPoint = bonus, TotalPoint = basePoint + bonus,
                        Message = "정답이에요! 보너스 포인트가 추가됐어요 🎉"
                    };
                }
                else
                {
                    // 여전히 오답 → 재도전 유도 (추가 점수 없음)
                    return new LessonSubmitResult
                    {
                        Ok = true, AlreadySubmitted = false, IsAllCorrect = false,
                        BasePoint = 0, BonusPoint = 0, TotalPoint = 0,
                        Message = "아직 틀린 빈칸이 있어요. 다시 도전해봐요! "
                    };
                }
            }

            // ── 케이스 2: 최초 제출 ──
            bool isAllCorrect = check.IsCorrect;
            int firstBonus = isAllCorrect ? 2 : 0;
            int total = basePoint + firstBonus;

            // Draft → Finalize 시도
            var fin = _repo.FinalizeSubmitFromDraft(lessonNo, userId, userName, basePoint);
            if (fin.updated)
            {
                return new LessonSubmitResult
                {
                    Ok = true, AlreadySubmitted = false,
                    IsAllCorrect = fin.isAllCorrect,
                    BasePoint = fin.basePoint, BonusPoint = fin.bonusPoint, TotalPoint = fin.totalPoint,
                    Message = fin.isAllCorrect
                        ? "제출 완료! 정답 보너스까지 적립됐어요 🎉"
                        : "제출 완료! 포인트가 적립됐어요. 틀린 빈칸을 고쳐서 보너스를 받아봐요! "
                };
            }

            // Fallback: 직접 Insert
            try
            {
                _repo.InsertSubmitHistory(lessonNo, userId, userName, isAllCorrect, basePoint, firstBonus, total);
            }
            catch
            {
                return new LessonSubmitResult
                {
                    Ok = false, AlreadySubmitted = false,
                    Message = "제출 저장 중 문제가 발생했어요. 잠시 후 다시 시도해 주세요."
                };
            }

            return new LessonSubmitResult
            {
                Ok = true, AlreadySubmitted = false, IsAllCorrect = isAllCorrect,
                BasePoint = basePoint, BonusPoint = firstBonus, TotalPoint = total,
                Message = isAllCorrect
                    ? "제출 완료! 정답 보너스까지 적립됐어요 🎉"
                    : "제출 완료! 포인트가 적립됐어요. 틀린 빈칸을 고쳐서 보너스를 받아봐요! "
            };
        }

        // ✅ 안전한 1글자 정규화 (룬이 없을 때 "\0" 방지)
        private static string NormalizeOneChar(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            s = s.Trim();

            foreach (var r in s.EnumerateRunes())
                return r.ToString();

            return "";
        }
    }
}
