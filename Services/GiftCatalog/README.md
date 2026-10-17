# GiftCatalog — 선물 품목 조회 사용법

선물 품목(`AwardItem`)을 찾는 일은 **이 폴더 두 파일**로만 한다.

| 파일 | 하는 일 |
|---|---|
| `GiftPriceRule.cs` | 가격 규칙 (목록 종류별 범위, 점수별 상한, "몇 점 이상" 문구) |
| `GiftCatalogService.cs` | 품목 조회 · 선택 가능 여부 검사 |

> 새로 SQL 을 쓰거나 `_db.AwardItems` 를 직접 조회하지 말 것.
> 가격 기준이 바뀌면 `GiftPriceRule` **한 파일만** 고친다.

---

## 1. 주입

```csharp
public MyController(BulletinBoard.Services.GiftCatalog.IGiftCatalogService giftCatalog)
{
    _giftCatalog = giftCatalog;
}
```

Razor 뷰에서 쓸 일이 있으면 `@inject`, 서비스에서는 생성자 주입. 이미 `Startup.cs` 에 등록돼 있다.

---

## 2. 메서드 3개

```csharp
Task<List<AwardItem>> GetAsync(GiftCatalogQuery query);            // 목록 (금액 오름차순)
Task<AwardItem?>      GetByIdAsync(int productId, bool includeHidden = false);  // 한 건
Task<bool>            CanPickAsync(int productId, GiftCatalogQuery query);      // 고를 수 있나?
```

`GetAsync` 는 항상 **숨김 제외(기본) · 금액 오름차순**이고, `PointRequirement`("700점이상" 같은 문구)를 채워서 돌려준다.

---

## 3. 조건 만들기 — `GiftCatalogQuery`

| 속성 | 뜻 |
|---|---|
| `Option` | 목록 종류 (`birth` · `newComer` · `display` · `FamilyTeam3K` · `challengeMissionBible` · `managePrice` …) |
| `MyPoint` | 점수로 상한이 정해지는 목록에서 내 점수 |
| `MaxUnitPrice` | 종류와 무관하게 "이 금액 이하"만 (완독 선물 한도 등) |
| `ProductId` | 한 건만 |
| `IncludeHidden` | 숨긴 품목 포함 (관리 화면만) |
| `IgnorePriceRule` | 가격 제한을 걸지 않음 (전체 목록) |

지름길 3개:

```csharp
GiftCatalogQuery.All()                  // 숨김 제외 전체
GiftCatalogQuery.All(includeHidden:true)// 관리 화면용 전체
GiftCatalogQuery.ById(1342)             // 한 건
GiftCatalogQuery.ByLimit(15000)         // 15,000원 이하
```

---

## 4. 자주 쓰는 예

**생일 선물 목록**

```csharp
var items = await _giftCatalog.GetAsync(new GiftCatalogQuery { Option = ProductDisplayOpton.birth });
```

**도전! 성경말씀 — 내 점수로 고를 수 있는 것만**

```csharp
var items = await _giftCatalog.GetAsync(new GiftCatalogQuery
{
    Option  = ProductDisplayOpton.challengeMissionBible,
    MyPoint = myPoint          // 100/400/700/1000/1500/2000점 구간에 따라 상한이 달라진다
});
```

**가족팀 합산 선물 (3,000점 = 3만원 미만)**

```csharp
var option = ProductDisplayOptonHelper.FamilyTeamOption(threshold);   // null 이면 박수 단계(선물 없음)
var items  = await _giftCatalog.GetAsync(new GiftCatalogQuery { Option = option.Value });
```

**성경통독 완독 선물 (기준표의 한도 사용)**

```csharp
var items = await _giftCatalog.GetAsync(GiftCatalogQuery.ByLimit(cfg.MaxUnitPrice));
```

**관리 화면 — 숨긴 것까지 한 건 조회**

```csharp
var item = await _giftCatalog.GetByIdAsync(productId, includeHidden: true);
```

**저장 전 서버 검사 (POST 에서 반드시)**

```csharp
var ok = await _giftCatalog.CanPickAsync(productId, new GiftCatalogQuery { Option = option.Value });
if (!ok) return 에러;
```

화면에서 못 고르게 막는 것과 별개로, **POST 는 항상 `CanPickAsync` 로 한 번 더 확인**한다.

---

## 5. 가격 규칙을 바꿀 때

`GiftPriceRule.cs` 만 고친다.

```csharp
GiftPriceRule.For(option, myPoint)      // → (Min, Max)
GiftPriceRule.MaxPriceByPoint(myPoint)  // → 점수별 상한
GiftPriceRule.PointRequirementLabel(p)  // → "700점이상"
GiftPriceRule.PointRequirementCaseSql() // → SQL 안에서 같은 문구를 만들 때
GiftPriceRule.WhereSql(option, myPoint) // → 옛 ADO 코드용 WHERE 조각
```

지금 기준

| 목록 | 가격 |
|---|---|
| 도전! 성경말씀 | 100점↑ 5천 / 400점↑ 1만 / 700점↑ 1.5만 / 1000점↑ 2만 / 1500점↑ 2.5만 / 2000점↑ 3만 |
| 생일 | 1,000원 ~ `ConstatsCommon.birthDayPresentMax` |
| 새친구 | 1,000 ~ 15,000원 |
| 진열 | 15,000 ~ 28,000원 |
| Special | 5,000원 미만 |
| 가족팀 2K/3K/4K/5K | 2만/3만/4만/5만원 미만 |
| managePrice | 금액 제한 없음, 설명이 비어 있는 품목만 |
