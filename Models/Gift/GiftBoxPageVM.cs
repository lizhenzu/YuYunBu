using System;
using System.Collections.Generic;
using System.Linq;

namespace BulletinBoard.Models.Gift
{
    /// <summary>
    ///  선물함(/StudentPointDashboard4/MyGiftBox3) 한 칸.
    ///  선물 종류가 달라도 화면에서는 같은 모양으로 그리기 위해, 컨트롤러에서 여기로 맞춰 담는다.
    /// </summary>
    public sealed class GiftBoxCardVM
    {
        /// <summary>point · bible · birth · family</summary>
        public string Type { get; set; } = "";

        public string Title  { get; set; } = "";   // 나의 도토리 선물
        public string Reason { get; set; } = "";   // 왜 받는 선물인지 한 줄

        /// <summary>지금 고를 수 있으면 true. false 면 흐린 칸 + 받는 방법 안내</summary>
        public bool IsOpen { get; set; }

        /// <summary>잠긴 칸을 눌렀을 때 펼쳐지는 안내</summary>
        public string LockHint { get; set; } = "";

        // 칸 오른쪽 숫자 (한도 · 남은 날 · 달성 단계 …)
        public string RightLabel { get; set; } = "";
        public string RightValue { get; set; } = "";
        public string RightUnit  { get; set; } = "";

        // 지금 고른 선물
        public int?   PickedProductId { get; set; }
        public string PickedName      { get; set; } = "";
        public string PickedMeta      { get; set; } = "";   // 12,000 credit · 선택 중
        public bool   HasPick => PickedProductId.HasValue;

        /// <summary>관리자가 주문을 마치면 false — 바꾸기·취소가 사라진다</summary>
        public bool CanChange { get; set; }

        /// <summary>대상자가 아니라 고를 수 없을 때 보여 줄 문구 (가족 선물)</summary>
        public string NotOwnerText { get; set; } = "";

        public string SelectUrl { get; set; } = "";   // 고르기 화면 주소

        // 취소 폼 (종류마다 보낼 곳과 값이 다르다)
        public string CancelAction { get; set; } = "";
        public Dictionary<string, string> CancelFields { get; set; } = new();
    }

    /// <summary>지난 선물 한 줄 — 4종을 한 목록에 모아 날짜순으로 보여 준다</summary>
    public sealed class GiftHistoryRowVM
    {
        public DateTime Date        { get; set; }
        public int?     ProductId   { get; set; }
        public string   ProductName { get; set; } = "";
        public string   Source      { get; set; } = "";   // 나의 도토리 900점 · 성경통독 1독 · 생일 · 우리 가족
        public string   Status      { get; set; } = "";   // 선택 중 · 준비 중 · 받음
    }

    public sealed class GiftBoxPageVM
    {
        public string FullName { get; set; } = "";
        public bool   IsStudent { get; set; }

        public List<GiftBoxCardVM>    Cards   { get; set; } = new();
        public List<GiftHistoryRowVM> History { get; set; } = new();

        public int OpenCount   => Cards.Count(c => c.IsOpen);
        public int PickedCount => Cards.Count(c => c.HasPick);
    }

    /// <summary>
    ///  선물 고르기(/StudentPointDashboard4/GiftSelect) — type 값에 따라 머리말만 달라진다.
    ///  (Services.Abstract 에 같은 이름이 이미 있어 Pick 으로 구분한다)
    /// </summary>
    public sealed class GiftPickPageVM
    {
        public string Type   { get; set; } = "";
        public string Kicker { get; set; } = "";   // 도전! 성경통독 완독 선물
        public string Title  { get; set; } = "";   // 성경 1독을 마쳤어요
        public string Why    { get; set; } = "";   // 왜 받는 선물인지

        public List<AwardItem> Items { get; set; } = new();
        public int Count    => Items.Count;
        public int MinPrice => Items.Count == 0 ? 0 : Items.Min(i => i.UnitPrice);
        public int MaxPrice => Items.Count == 0 ? 0 : Items.Max(i => i.UnitPrice);

        /// <summary>고르기 폼이 보낼 곳 (기존 액션을 그대로 쓴다)</summary>
        public string PostAction { get; set; } = "";
        /// <summary>productId 말고 같이 보내야 하는 값들 (round · teamName …)</summary>
        public Dictionary<string, string> PostFields { get; set; } = new();

        /// <summary>이미 고른 선물이 있으면 버튼이 '바꾸기'가 된다</summary>
        public int? PickedProductId { get; set; }

        /// <summary>고를 게 하나도 없을 때 보여 줄 안내 (없으면 기본 문구)</summary>
        public string? EmptyText { get; set; }
    }
}
