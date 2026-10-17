using System.Threading.Tasks;

namespace BulletinBoard.Services.GiftAdmin
{
    /// <summary>
    ///  선물 관리 한 창구. 도토리 · 가족팀 · 가족 합산 · 성경통독을 한 화면에서 다루게 모아 준다.
    ///  테이블은 그대로 두고, 읽을 때 합치고 쓸 때 종류대로 나눠 보낸다.
    /// </summary>
    public interface IGiftAdminFacade
    {
        /// <summary><paramref name="deliveredKind"/> 는 전달 완료 묶음에만 건다. 미처리는 언제나 전부.</summary>
        Task<UnifiedGiftAdminPageVM> GetPageAsync(GiftKind? deliveredKind, bool isSuperAdmin);

        Task<(bool ok, string msg)> PurchaseAsync(GiftKind kind, int orderId, string adminId);
        Task<(bool ok, string msg)> UndoPurchaseAsync(GiftKind kind, int orderId, string adminId);
        Task<(bool ok, string msg)> DeliverAsync(GiftKind kind, int orderId, string adminId);
        Task<(bool ok, string msg)> UndoDeliverAsync(GiftKind kind, int orderId, string adminId);
    }
}
