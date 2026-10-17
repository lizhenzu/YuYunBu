-- Day 29 : 분량 보강 과정에서 "자기 몫을" 이 한 문단에 두 번 나왔다. 하나를 고친다.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

UPDATE dbo.PrayPhraseTemplate SET PrayContent1 = REPLACE(PrayContent1,
 N'하나님 앞에서 자기 몫을 스스로 답할 줄 아는 아이로 자라게 하시고, 작은 약속도 지키게 하소서.',
 N'하나님 앞에서 자기가 한 일을 스스로 답할 줄 아는 아이로 자라게 하시고, 작은 약속도 지키게 하소서.')
WHERE DayNo = 29;

COMMIT TRAN;
PRINT '[PrayPhraseTemplate] Day 29 중복 표현 정리';
GO
