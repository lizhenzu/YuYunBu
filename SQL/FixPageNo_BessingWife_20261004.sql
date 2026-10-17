-- ============================================================
--  아내를 위한 남편의 기도(BessingWife) — 장 번호(PageNo) 중복 정리 (2026-10-04)
--
--  60편의 장 번호는 8 ~ 126 사이 짝수인데, 116 이 두 편에 겹치고 34 가 비어 있습니다.
--    · Id 24 "아내의 은사와 부르심을 위한 기도"  116  → 그대로 (114 성령의 열매 · 118 분별 과 같은 신앙 묶음)
--    · Id 25 "부부의 대화를 회복하게 하소서"      116  → 34  (비어 있던 번호)
--
--  ⚠️ PageNo 한 칸만 바꿉니다. 제목 · 본문 · 말씀 · 수정일 · 조회수는 그대로.
--  ⚠️ 제목과 현재 번호가 일치하는 행만 바꾸므로, 다시 실행해도 0건으로 지나갑니다.
--  되돌리기:  UPDATE dbo.BessingWife SET PageNo = 116 WHERE Id = 25;
-- ============================================================
SET NOCOUNT ON;

UPDATE dbo.BessingWife
   SET PageNo = 34
 WHERE Id = 25 AND PageNo = 116 AND PrayTitle = N'부부의 대화를 회복하게 하소서';
PRINT N'Id 25 장 번호 116 → 34 — ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N'건';

-- 확인: 겹치는 번호가 남아 있으면 아래에 나옵니다 (아무것도 안 나오면 정상)
SELECT PageNo, COUNT(*) AS Cnt FROM dbo.BessingWife GROUP BY PageNo HAVING COUNT(*) > 1;
