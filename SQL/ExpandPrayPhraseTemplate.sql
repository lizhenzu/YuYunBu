-- ============================================================
--  PrayContent1 분량 보강 — 마지막 간구 문장을 원문 길이에 맞게 늘린다
--  (문단 구조는 그대로, 세 번째 줄만 확장)
-- ============================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

UPDATE dbo.PrayPhraseTemplate SET PrayContent1 = REPLACE(PrayContent1,
 N'견디어 낸 자리마다 믿음이 한 뼘씩 자라나게 하소서.',
 N'견디어 낸 자리마다 믿음이 한 뼘씩 자라나게 하시고, 그 인내가 훗날의 힘이 되게 하소서.') WHERE DayNo = 2;

UPDATE dbo.PrayPhraseTemplate SET PrayContent1 = REPLACE(PrayContent1,
 N'용서받은 사람으로 살아가는 기쁨을 어려서부터 알게 하소서.',
 N'용서받은 사람으로 살아가는 기쁨을 어려서부터 알게 하시고, 그 마음이 관계를 살리게 하소서.') WHERE DayNo = 4;

UPDATE dbo.PrayPhraseTemplate SET PrayContent1 = REPLACE(PrayContent1,
 N'스스로를 다스리는 법을 배워 훗날 자유롭게 하소서.',
 N'스스로를 다스리는 법을 어려서 배워, 훗날 그 절제가 참된 자유가 되게 하소서.') WHERE DayNo = 5;

UPDATE dbo.PrayPhraseTemplate SET PrayContent1 = REPLACE(PrayContent1,
 N'그 지혜가 곁에 있는 이들에게 도움이 되게 하소서.',
 N'그 지혜가 곁에 있는 이들에게 도움이 되고, 하나님을 아는 길로 이어지게 하소서.') WHERE DayNo = 6;

UPDATE dbo.PrayPhraseTemplate SET PrayContent1 = REPLACE(PrayContent1,
 N'나눔이 손해가 아니라 하나님께 드리는 기쁨임을 알게 하소서.',
 N'나눔이 손해가 아니라 하나님께 드리는 예배임을 알게 하시고, 그 기쁨을 자주 누리게 하소서.') WHERE DayNo = 8;

UPDATE dbo.PrayPhraseTemplate SET PrayContent1 = REPLACE(PrayContent1,
 N'섬기시려고 오신 예수님을 닮아 가게 하소서.',
 N'섬기려고 오신 예수님을 닮아 가게 하시고, 사람을 세우는 그 자리에서 참된 기쁨을 얻게 하소서.') WHERE DayNo = 9;

UPDATE dbo.PrayPhraseTemplate SET PrayContent1 = REPLACE(PrayContent1,
 N'남의 형편을 헤아리는 눈이 자라나게 하소서.',
 N'남의 형편을 헤아리는 눈이 자라나게 하시고, 그 마음이 오래도록 식지 않게 지켜 주소서.') WHERE DayNo = 10;

UPDATE dbo.PrayPhraseTemplate SET PrayContent1 = REPLACE(PrayContent1,
 N'저희 또한 순종하고 싶은 부모가 되도록 저희를 다듬어 주소서.',
 N'저희 또한 순종하고 싶은 부모가 되도록 다듬어 주시고, 저희 말이 주님의 뜻에서 벗어나지 않게 하소서.') WHERE DayNo = 11;

UPDATE dbo.PrayPhraseTemplate SET PrayContent1 = REPLACE(PrayContent1,
 N'그 따뜻함이 자라 누군가의 피난처가 되게 하소서.',
 N'그 따뜻함이 자라 누군가에게 피난처가 되게 하시고, 상처받은 이를 먼저 알아보게 하소서.') WHERE DayNo = 13;

UPDATE dbo.PrayPhraseTemplate SET PrayContent1 = REPLACE(PrayContent1,
 N'날마다 새로워져 예수님의 모습이 드러나게 하소서.',
 N'날마다 새로워져 **○○(이)**의 말과 행동에 예수님의 모습이 자연스럽게 드러나게 하소서.') WHERE DayNo = 16;

UPDATE dbo.PrayPhraseTemplate SET PrayContent1 = REPLACE(PrayContent1,
 N'약해지는 자리에서 주님을 의지하는 법을 배우게 하소서.',
 N'약해지는 그 자리에서 오히려 주님을 더 깊이 의지하는 법을 배우게 하소서.') WHERE DayNo = 17;

UPDATE dbo.PrayPhraseTemplate SET PrayContent1 = REPLACE(PrayContent1,
 N'누가 보든 보지 않든 같은 마음으로 하게 하소서.',
 N'누가 보든 보지 않든 같은 마음으로 하게 하시고, 그 성실을 주님이 기억하심을 믿게 하소서.') WHERE DayNo = 18;

UPDATE dbo.PrayPhraseTemplate SET PrayContent1 = REPLACE(PrayContent1,
 N'두려워하는 마음이 아니라 능력과 사랑과 절제의 마음을 주소서.',
 N'두려워하는 마음이 아니라 능력과 사랑과 절제의 마음을 주시고, 그 담대함으로 선한 길을 택하게 하소서.') WHERE DayNo = 20;

UPDATE dbo.PrayPhraseTemplate SET PrayContent1 = REPLACE(PrayContent1,
 N'기도가 마지막이 아니라 처음이 되게 하소서.',
 N'기도가 마지막에 찾는 수단이 아니라 가장 먼저 나아가는 자리가 되게 하소서.') WHERE DayNo = 21;

UPDATE dbo.PrayPhraseTemplate SET PrayContent1 = REPLACE(PrayContent1,
 N'두려움이 아니라 사랑으로 주님을 높이게 하소서.',
 N'두려움이 아니라 사랑으로 주님을 높이게 하시고, 그 경외가 삶의 중심이 되게 하소서.') WHERE DayNo = 23;

UPDATE dbo.PrayPhraseTemplate SET PrayContent1 = REPLACE(PrayContent1,
 N'힘 주시는 분이 주님이심을 알아 담대히 나아가게 하소서.',
 N'힘 주시는 분이 주님이심을 알게 하시고, 그 믿음으로 새로운 일 앞에 담대히 나아가게 하소서.') WHERE DayNo = 24;

UPDATE dbo.PrayPhraseTemplate SET PrayContent1 = REPLACE(PrayContent1,
 N'그 입술이 믿을 만한 사람으로 자라게 하소서.',
 N'그 입술로 인해 어디서나 믿을 만한 사람으로 자라게 하시고, 그 말에 무게가 있게 하소서.') WHERE DayNo = 26;

UPDATE dbo.PrayPhraseTemplate SET PrayContent1 = REPLACE(PrayContent1,
 N'듣기는 빨리 하고 말하기는 천천히 하는 아이가 되게 하소서.',
 N'듣기는 빨리 하고 말하기는 천천히 하며 성내기도 더디 하는 아이가 되게 하소서.') WHERE DayNo = 27;

UPDATE dbo.PrayPhraseTemplate SET PrayContent1 = REPLACE(PrayContent1,
 N'하나님 앞에서 스스로 답할 줄 아는 아이로 자라게 하소서.',
 N'하나님 앞에서 자기 몫을 스스로 답할 줄 아는 아이로 자라게 하시고, 작은 약속도 지키게 하소서.') WHERE DayNo = 29;

UPDATE dbo.PrayPhraseTemplate SET PrayContent1 = REPLACE(PrayContent1,
 N'그 결단이 하나님의 뜻 위에 세워지게 하소서.',
 N'그 결단이 자기 고집이 아니라 하나님의 뜻 위에 세워지게 하시고, 한번 정한 길을 끝까지 지켜 내게 하소서.') WHERE DayNo = 30;

UPDATE dbo.PrayPhraseTemplate SET PrayContent1 = REPLACE(PrayContent1,
 N'가르침을 마음에 새겨 삶으로 옮기는 아이가 되게 하소서.',
 N'가르침을 귀담아듣고 마음에 새겨, 배운 대로 살아 내는 아이로 자라게 하소서.') WHERE DayNo = 31;

COMMIT TRAN;
PRINT '[PrayPhraseTemplate] 분량 보강 완료';
GO
