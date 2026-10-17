using BulletinBoard.Models.Jubo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BulletinBoard.Services.Jubo
{
    public interface IJuboService
    {
        Task EnsureTablesAsync();

        // Jubo CRUD
        Task<JuboListViewModel> GetJuboListAsync(int page = 1, int pageSize = 12);
        Task<JuboEditViewModel?> GetJuboForEditAsync(int id);
        Task<JuboPreviewViewModel?> GetJuboForPreviewAsync(int id);
        Task<int> CreateJuboAsync(JuboCreateViewModel model, string userName);
        Task<bool> UpdateJuboMetaAsync(int id, string title, string? season, string templateType, string userName);
        Task<bool> DeleteJuboAsync(int id);
        Task<bool> TogglePublishAsync(int id, string userName);

        // Blocks
        Task<bool> SaveBlocksAsync(int juboId, List<BlockEditViewModel> blocks, string userName);
        Task<bool> SaveSingleBlockAsync(BlockEditViewModel block, string userName);

        // Snapshot
        Task<int> CreateSnapshotAsync(int juboId, string userName);
        Task<SundaySchoolJuboSnapshot?> GetLatestSnapshotAsync(int juboId);

        // Attendance
        Task<AttendanceSummaryViewModel> GetAttendanceSummaryAsync(DateTime date);
        Task<bool> SaveAttendanceClassAsync(ClassAttendanceSaveItem item, DateTime date, string userName);
        Task<bool> DeleteAttendanceClassAsync(int id);
        Task<List<AttendanceNewComerModel>> GetNewComersAsync(DateTime date);
        Task<int> SaveNewComerAsync(SaveNewComerViewModel model, string userName);
        Task<bool> DeleteNewComerAsync(int id);
    }
}
