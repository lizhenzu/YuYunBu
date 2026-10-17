using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BulletinBoard.DataAccessLayer.Birthday;
using BulletinBoard.DataAccessLayer.Birthday.Models;
using BulletinBoard.ViewModels.Birthday;
using Microsoft.AspNetCore.Hosting;
using BulletinBoard.Enums; // Roles 등 쓰실 경우 대비

namespace BulletinBoard.Services
{
    public class BirthdayService : Abstract.IBirthdayService
    {
        private readonly IBirthdayRepository _repo;
        private readonly IWebHostEnvironment _env;

        public BirthdayService(IBirthdayRepository repo, IWebHostEnvironment env)
        {
            _repo = repo;
            _env = env;
        }

        // -------------------------------
        // ✅ 선물 상태 저장(PresentItemPick)
        // -------------------------------
        public async Task<int> UpdateGiftStateAsync(string userId, bool isActive, bool isSent)
        {
            return await _repo.UpdateGiftStateAsync(userId, isActive, isSent);
        }

        // -------------------------------
        // ✅ 선물 선택 요청 문자(SMS) 발송 여부 저장
        // -------------------------------
        public async Task<int> UpdatePickSmsStateAsync(string userId, bool smsSent)
        {
            return await _repo.UpdatePickSmsStateAsync(userId, smsSent);
        }

        // -------------------------------
        // ✅ 메모 업서트: BirthdayGiftPickMeMo (PersonID 키)
        // -------------------------------
        public async Task<int> UpsertMemoAsync(int personId, string? memo)
        {
            return await _repo.UpsertMemoAsync(personId, memo);
        }

        public async Task<BirthdayListPageVM> GetNoBirthdayAsync()
        {
            var rows = await _repo.GetNoBirthdayListAsync();
            var items = rows.Select(ToCard).ToList();

            return new BirthdayListPageVM
            {
                Month = null,
                PageTitle = "생일 미지정 명단",
                Items = items
            };
        }

        private string ResolveProfileUrl(string? userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return "/images/icon/people/people_01.webp";

            var physical = Path.Combine(_env.WebRootPath, "images", "Profile", $"{userId}.png");
            return File.Exists(physical)
                ? $"/images/Profile/{userId}.png"
                : "/images/icon/people/people_01.webp";
        }

        // RoleLabel / ClassLabel은 현재 테이블이 없어서 “기본값”으로만.
        // 원하시면 RoleCode/ClassYear/ClassBan를 view에 포함시키거나,
        // ClassInfo_2026과 조인해서 확정해드릴 수 있어요.
        private string GuessRoleLabel(ViewPersonMaster2026Row r)
        {
            // 반 정보가 있으면 학생으로 보는 것이 가장 자연스러움
            if (r.ClassYear.HasValue && r.ClassYear.Value > 0) return "학생";
            return "대상"; // RoleCode 없으니 확정 대신 안전 라벨
        }

        private string GuessClassLabel(ViewPersonMaster2026Row r)
        {
            if (r.ClassYear.HasValue && r.ClassYear.Value > 0)
            {
                var ban = r.ClassBan.HasValue ? r.ClassBan.Value.ToString() : "";
                return string.IsNullOrWhiteSpace(ban)
                    ? $"{r.ClassYear}학년"
                    : $"{r.ClassYear}학년 {ban}반";
            }
            return "";
        }

        private BirthdayCardVM ToCard(ViewPersonMaster2026Row r)
        {
            // ===========================
            // ✅ 선물 선택 여부 추천 기준(A)
            // - PickId가 있고 IsActive=1 일 때만 "선택완료"
            // ===========================
            var hasPick = (r.PickId.HasValue && r.PickId.Value > 0);
            var isActivePick = (r.PickIsActive ?? false);
            var giftPicked = hasPick && isActivePick;

            return new BirthdayCardVM
            {
                PersonId = r.PersonId,
                UserId = r.UserId ?? "",
                FullName = r.fullName ?? "",
                BMonth = r.bMonth,
                BDay = r.bDay,
                PublishYN = r.birthDay_PublishYN ?? "",
                PhoneNumber = r.PhoneNumber,
                PhoneNumber2 = r.PhoneNumber2,
                RoleLabel = GuessRoleLabel(r),
                ClassLabel = GuessClassLabel(r),
                ProfileImageUrl = ResolveProfileUrl(r.UserId),

                // ✅ 선물 선택 정보 매핑
                GiftPicked = giftPicked,
                GiftProductId = r.PickProductId,
                GiftProductName = r.PickProductName,
                GiftProductDescription = r.PickProductDescription,
                GiftPickedAt = r.PickedAt,
                GiftIsActive = isActivePick,              // null이면 false
                GiftIsSent = (r.PickIsSent ?? false),
                GiftSentAt = r.PickSentAt,
                GiftSourceType = r.PickSourceType,

                // ✅ 추가: 선물 선택 요청 문자(SMS) 발송 여부
                PickSmsSent = (r.PickSmsSent ?? false),
                PickSmsSentAt = r.PickSmsSentAt,

                // ✅ 메모 / 비고
                Memo = r.PickMemo
            };
        }

        public async Task<BirthdayListPageVM> GetMonthAsync(int month)
        {
            var rows = await _repo.GetListAsync(month);
            var items = rows.Select(ToCard).ToList();

            return new BirthdayListPageVM
            {
                Month = month,
                PageTitle = $"{month}월 생일자",
                Items = items
            };
        }

        public async Task<BirthdayListPageVM> GetYearAsync()
        {
            var rows = await _repo.GetListAsync(null);
            var items = rows.Select(ToCard).ToList();

            var group = items
                .Where(x => x.BMonth.HasValue && x.BMonth.Value is >= 1 and <= 12)
                .GroupBy(x => x.BMonth!.Value)
                .OrderBy(g => g.Key)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(x => x.BDay ?? 99).ThenBy(x => x.FullName).ToList()
                );

            return new BirthdayListPageVM
            {
                Month = null,
                PageTitle = "연간 생일자(1~12월)",
                Items = items,
                GroupByMonth = group
            };
        }

        public async Task<BirthdayEditVM?> GetEditAsync(int personId)
        {
            var r = await _repo.GetOneAsync(personId);
            if (r == null) return null;

            return new BirthdayEditVM
            {
                PersonId = r.PersonId,
                UserId = r.UserId ?? "",
                UserName = r.UserName ?? "",
                FullName = r.fullName ?? "",
                Sex = r.sex,
                PhoneNumber = r.PhoneNumber,
                PhoneNumber2 = r.PhoneNumber2,
                HomeAaddress = r.HomeAaddress,
                School = r.school,
                ProfileImageUrl = ResolveProfileUrl(r.UserId),

                BYear = r.bYear,
                BMonth = r.bMonth,
                BDay = r.bDay,
                PlusMins = r.plusMins,
                PublishYN = string.IsNullOrWhiteSpace(r.birthDay_PublishYN) ? "Y" : r.birthDay_PublishYN!
            };
        }
    }
}
