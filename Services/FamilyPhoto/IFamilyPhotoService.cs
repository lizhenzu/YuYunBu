using BulletinBoard.Models.FamilyPhoto;
using Microsoft.AspNetCore.Http;

namespace BulletinBoard.Services.FamilyPhoto;

/// <summary>추석맞이 우리 가족 사진 콘테스트 — 갤러리 · 참가 · 좋아요 · 결과 · 관리</summary>
public interface IFamilyPhotoService
{
    /// <summary>표가 없으면 만든다 (프로세스당 한 번)</summary>
    Task EnsureTablesAsync();

    // ── 보기 ──
    Task<FamilyPhotoGalleryVM> GetGalleryAsync(string deviceKey, bool hasMyEntry, bool isStaff = false);
    Task<FamilyPhotoBannerVM>  GetBannerAsync();
    Task<FamilyPhotoResultVM>  GetResultAsync(bool preview);
    Task<bool>                 IsPublishedAsync();

    // ── 참가 (가족명 + PIN) ──
    /// <summary>있으면 PIN 확인, 없으면 새로 만든다 (여는 날부터 언제든 — 마감 없음). ok=false 면 msg 에 이유</summary>
    Task<(bool ok, string msg, int entryId)> EnterAsync(string familyName, string pin, string? ip);
    Task<FamilyPhotoManageVM?> GetManageAsync(int entryId, string deviceKey);
    Task<(bool ok, string msg)> UploadAsync(int entryId, int slot, IFormFile file, string? caption);
    Task<(bool ok, string msg)> SetCaptionAsync(int entryId, int slot, string? caption);
    Task<(bool ok, string msg)> RemovePhotoAsync(int entryId, int slot);
    Task<(bool ok, string msg)> DeleteEntryAsync(int entryId);

    // ── 좋아요 ──
    /// <summary>누르면 켜지고, 다시 누르면 꺼진다. liked = 결과 상태</summary>
    Task<(bool ok, string msg, bool liked)> ToggleLikeAsync(int photoId, string deviceKey, string? ip, bool isStaff = false);

    // AI 리뷰 보드 스티커
    Task<FamilyPhotoStickerVM> GetStickersAsync(string? deviceKey);
    Task<(bool ok, string msg, int mine, int[] counts)> SetStickerAsync(int photoId, int kind, string deviceKey, string? ip);

    // ── 관리자 ──
    Task<FamilyPhotoAdminVM> GetAdminAsync();
    Task<(bool ok, string msg)> SetEntryHiddenAsync(int entryId, bool hidden, string adminName);
    Task<(bool ok, string msg)> SetPhotoHiddenAsync(int photoId, bool hidden, string adminName);
    /// <summary>새 PIN 4자리를 만들어 돌려준다 (한 번만 보여 준다)</summary>
    Task<(bool ok, string msg, string newPin)> ResetPinAsync(int entryId, string adminName);
    Task<(bool ok, string msg)> SetPublishedAsync(bool published, string adminName);
    /// <summary>가족 · 사진 · 좋아요 · 파일 전부 지우고 발표도 되돌린다 (테스트 자료 초기화용, SuperAdmin)</summary>
    Task<(bool ok, string msg)> ResetAllAsync(string adminName);
}
