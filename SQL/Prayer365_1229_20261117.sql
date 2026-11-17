-- 자녀를 위한 365일 축복 기도문 (한치호 지음 · 도서출판 두돌비) — 12/29 (1편)
-- 2026-11-17 · 사용자가 붙여 넣은 글자 인식 결과를 Claude 가 교정 (오타 · 이상한 문구만) · IsReviewed = 1
-- 같은 날짜가 이미 있으면 덮어씁니다. 여러 번 실행해도 안전합니다.
SET XACT_ABORT ON;
BEGIN TRAN;

MERGE dbo.Prayer365 AS t
USING (VALUES
('1229', N'주님의 뜻 안에서 살게 하옵소서', N'시 102:26-27',
 N'천지는 없어지려니와 주는 영존하시겠고 그것들은 다 옷 같이 낡으리니 의복 같이 바꾸시면 바뀌려니와 주는 여상하시고 주의 년대는 무궁하리이다',
 N'시작과 끝의 하나님,
저의 아이가 주님을 섬기면서 살아오게 하셨음을 즐거워합니다. 생각하건대 세상은 참으로 험하였으나 주님의 의로우신 팔로 저의 아이를 지켜 주셨습니다. 때를 따라서 도와주시고, 시절에 따라 열매를 맺어 기쁨을 누리게 하셨습니다. 올해 1년은 하나님의 시간이었습니다. 저의 아이를 위하여 하나님은 큰일을 해주셨습니다.
하나님 아버지,
지난 한 해 동안에 주님을 실망시킨 여러 가지 일을 생각할 때 부끄러움이 큽니다. 하나님 아버지를 즐겁게 하지 못하는 것을 알고도 행하였으며 여호와 앞에 영화로움을 드리는 것을 알고도 행치 못했습니다. 또한, 쓸데없는 시간을 낭비했으며 여러 기회를 놓치고 살아왔습니다. 다가오는 새해에는 나로 주님의 뜻 안에서 살 수 있도록 도와주옵소서. 그들의 생명을 하나님의 나라를 위해서 가치 있게 살도록 은혜를 내려 주옵소서.
예배를 받으시는 하나님,
올해의 마지막 주일을 맞이합니다. 여호와의 사심과 여호와의 도와주시는 풍성함 속에서 지내온 저의 아이가 자신에게 있는 것 전부를 드려 감사하는 예배를 준비하게 하옵소서. 드려도, 드려도 못 다 드릴 은혜를 새롭게 하며 예배를 사모하는 마음을 주시기를 원하면서 예수님의 이름으로 기도드립니다. 아멘.',
 N'[{"From":"시 102:20-27","To":"시 102:26-27"},{"From":"없어지려너와","To":"없어지려니와"},{"From":"닭으리니","To":"낡으리니"},{"From":"바꾸 시면","To":"바꾸시면"},{"From":"생각 하건대","To":"생각하건대"},{"From":"지 켜","To":"지켜"},{"From":"기쁨 을","To":"기쁨을"},{"From":"아이 를","To":"아이를"},{"From":"하나님아버지","To":"하나님 아버지"},{"From":"부끄럽 큽니다","To":"부끄러움이 큽니다"},{"From":"쓸 데없는","To":"쓸데없는"},{"From":"생 명을","To":"생명을"},{"From":"하나님 ,","To":"하나님,"},{"From":"도와주 시는","To":"도와주시는"},{"From":"하읍소서","To":"하옵소서"},{"From":"이름 으로기도드립니다","To":"이름으로 기도드립니다"},{"From":"아멘,","To":"아멘."},{"From":"378 자녀를 위한 365일 축복 기도문","To":"(쪽 머리글 — 뺌)"}]')
) AS s (MonthDay, Title, VerseRef, VerseText, Body, Fixes)
ON t.MonthDay = s.MonthDay
WHEN MATCHED THEN UPDATE SET Title = s.Title, VerseRef = s.VerseRef, VerseText = s.VerseText, Body = s.Body, Fixes = s.Fixes,
     IsReviewed = 1, UpdatedAt = DATEADD(HOUR, 9, SYSUTCDATETIME()), UpdatedBy = N'Claude'
WHEN NOT MATCHED THEN INSERT (MonthDay, Title, VerseRef, VerseText, Body, Fixes, IsReviewed, UpdatedAt, UpdatedBy)
     VALUES (s.MonthDay, s.Title, s.VerseRef, s.VerseText, s.Body, s.Fixes, 1, DATEADD(HOUR, 9, SYSUTCDATETIME()), N'Claude');

COMMIT;

SELECT MonthDay, Title, VerseRef, IsReviewed, LEN(Body) AS BodyLen
FROM dbo.Prayer365 WHERE MonthDay = '1229';
