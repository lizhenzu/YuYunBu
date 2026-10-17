-- ============================================================
--  BessingWife 분량 보강 (Id 1~30)
--  마지막 "주님, ..." 문단 앞에 구체적 간구 문단을 한 개씩 삽입
-- ============================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;
DECLARE @P NVARCHAR(4) = CHAR(13)+CHAR(10)+CHAR(13)+CHAR(10);

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 저는 아내가 쉬지 못하는',
 N'잠들기 전 아내의 마음을 조용하게 하여 주옵소서. 오늘 하루 잘못된 일을 곱씹으며 자신을 탓하지 않게 하시고, 지나간 하루를 주님 손에 맡기고 눈을 감게 하옵소서. 잠자리에 드는 그 순간이 아내에게 하루 중 가장 편안한 시간이 되게 하옵소서.' + @P + N'주님, 저는 아내가 쉬지 못하는') WHERE Id = 1;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 아내가 지쳐 있을 때',
 N'한 가지 일을 마치고 다음 일로 넘어가기 전에 숨을 고를 틈을 주옵소서. 종일 서서 일한 다리와 무거운 것을 든 팔을 주님이 만져 주시고, 저녁이 되어도 바닥나지 않는 기운을 날마다 새로 부어 주옵소서.' + @P + N'주님, 아내가 지쳐 있을 때') WHERE Id = 2;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 제가 아내의 몸을 대신',
 N'해마다 받아야 할 검진을 미루지 않게 하시고, 바쁘다는 이유로 자기 몸을 뒷전에 두지 않게 하옵소서. 약을 먹어야 한다면 거르지 않게 하시고, 의사의 말을 가볍게 흘리지 않는 마음도 주옵소서.' + @P + N'주님, 제가 아내의 몸을 대신') WHERE Id = 3;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 아내가 아프다고 말할 때',
 N'참는 것이 익숙해져 아프다는 말조차 하지 않게 되지 않게 하옵소서. 아픈 곳을 숨기지 않고 말할 수 있게 하시고, 진통제로 하루하루를 넘기는 일이 습관이 되지 않게 하시며 근본을 고쳐 주옵소서.' + @P + N'주님, 아내가 아프다고 말할 때') WHERE Id = 4;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 밥상 앞에서 제가',
 N'식구들의 입맛을 맞추느라 정작 자신이 무엇을 좋아하는지 잊고 살지 않게 하옵소서. 아내가 먹고 싶은 것을 말할 수 있게 하시고, 그 말을 저희가 기쁘게 들어주는 가정이 되게 하옵소서.' + @P + N'주님, 밥상 앞에서 제가') WHERE Id = 5;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 제가 그 변화를 함부로',
 N'몸이 달라지는 시기에 마음까지 요동하지 않게 붙들어 주옵소서. 이유 없이 눈물이 나거나 화가 치미는 날에도 스스로를 이상하게 여기지 않게 하시고, 필요하다면 도움을 구할 지혜도 주옵소서.' + @P + N'주님, 제가 그 변화를 함부로') WHERE Id = 6;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 아내가 회복되기를 기다리는',
 N'회복의 과정에서 아내가 자신을 짐처럼 여기지 않게 하옵소서. 도움을 받는 것이 미안한 일이 아님을 알게 하시고, 곁에서 돌보는 손길을 감사로 받아들이는 마음을 주옵소서.' + @P + N'주님, 아내가 회복되기를 기다리는') WHERE Id = 7;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 아내가 걱정을 털어놓을 때',
 N'염려가 몰려오는 밤에 아내가 홀로 그것과 씨름하지 않게 하옵소서. 그 자리에서 주님의 이름을 부르게 하시고, 걱정하던 일들이 주님의 손안에서 어떻게 지나갔는지를 돌아보며 믿음이 자라게 하옵소서.' + @P + N'주님, 아내가 걱정을 털어놓을 때') WHERE Id = 8;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 제가 아내에게 힘내라는',
 N'가라앉은 날에도 아내가 자신을 챙기게 하옵소서. 끼니를 거르지 않게 하시고, 잠깐이라도 햇빛 아래를 걷게 하시며, 마음을 나눌 수 있는 사람에게 손 내밀 용기를 주옵소서.' + @P + N'주님, 제가 아내에게 힘내라는') WHERE Id = 9;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 아내의 억울함 가운데',
 N'화가 난 마음을 엉뚱한 곳에 쏟지 않게 하옵소서. 아이들에게 목소리가 높아진 날에는 곧 돌이켜 사과하게 하시고, 참았던 감정이 몸의 병으로 굳어지지 않게 지켜 주옵소서.' + @P + N'주님, 아내의 억울함 가운데') WHERE Id = 10;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 저는 아내와 한집에',
 N'아내가 마음을 열었다가 다치는 일이 반복되어 이제는 아예 닫아 버리지 않게 하옵소서. 다시 한번 사람을 믿어 볼 용기를 주시고, 먼저 안부를 묻는 사람이 되게 하옵소서.' + @P + N'주님, 저는 아내와 한집에') WHERE Id = 11;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 제 말과 표정이',
 N'해내지 못한 일을 세는 대신 해낸 일을 보게 하옵소서. 하루의 끝에 자신을 향해 오늘도 수고했다고 말할 수 있게 하시고, 완벽하지 않아도 사랑받는다는 사실을 마음으로 알게 하옵소서.' + @P + N'주님, 제 말과 표정이') WHERE Id = 12;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 제가 무심코 다른 사람과',
 N'다른 사람의 삶을 들여다보다 마음이 상하는 시간이 길어지지 않게 하옵소서. 부러움이 시기로 자라지 않게 하시고, 오히려 잘된 이를 위해 함께 기뻐해 줄 수 있는 넉넉한 마음을 주옵소서.' + @P + N'주님, 제가 무심코 다른 사람과') WHERE Id = 13;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 그 상처 중에 제가',
 N'그 기억이 불쑥 떠오르는 순간마다 주님이 곧바로 함께해 주옵소서. 혼자 삭이지 않게 하시고, 필요하다면 믿을 만한 사람에게 털어놓을 수 있는 용기와 그럴 사람을 허락하여 주옵소서.' + @P + N'주님, 그 상처 중에 제가') WHERE Id = 14;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 아내의 존귀함을 가장',
 N'자신을 위해 무언가를 사거나 시간을 쓰는 일에 죄책감을 느끼지 않게 하옵소서. 자신을 돌보는 것이 이기적인 일이 아니라 주님이 맡기신 몸을 지키는 일임을 알게 하옵소서.' + @P + N'주님, 아내의 존귀함을 가장') WHERE Id = 15;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 예배의 자리로 나아가는',
 N'주일 아침에 서두르다 마음이 상한 채로 예배당에 들어서지 않게 하옵소서. 집을 나서기 전부터 마음이 준비되게 하시고, 예배가 끝난 뒤에도 그 은혜가 한 주간 이어지게 하옵소서.' + @P + N'주님, 예배의 자리로 나아가는') WHERE Id = 16;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 제가 아내에게 말씀을',
 N'말씀을 읽을 자리와 시간을 정하게 하시고, 그 자리가 아내에게 익숙하고 편안한 곳이 되게 하옵소서. 읽은 것을 적어 두게 하시고, 그 기록이 훗날 주님의 인도하심을 확인하는 증거가 되게 하옵소서.' + @P + N'주님, 제가 아내에게 말씀을') WHERE Id = 17;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 제가 아내의 기도 제목을',
 N'기도가 부탁의 목록으로만 채워지지 않게 하옵소서. 구하기 전에 먼저 주님을 찬양하게 하시고, 잠잠히 주님의 음성을 기다리는 시간도 아내의 기도 안에 있게 하옵소서.' + @P + N'주님, 제가 아내의 기도 제목을') WHERE Id = 18;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 이런 때의 아내를',
 N'메마른 때일수록 홀로 떨어져 지내지 않게 하옵소서. 예배의 자리와 지체들 곁에 머물게 하시고, 은혜를 받은 이들의 이야기를 들으며 다시 목마름이 생기게 하옵소서.' + @P + N'주님, 이런 때의 아내를') WHERE Id = 19;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 그 오랜 기도 제목을',
 N'기다리는 동안 다른 방법으로 해결하려는 조급함에 빠지지 않게 하옵소서. 사람을 의지하거나 지름길을 찾지 않게 하시고, 주님이 하실 때까지 잠잠히 기다리는 믿음을 주옵소서.' + @P + N'주님, 그 오랜 기도 제목을') WHERE Id = 20;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 아내가 교회를 섬기는',
 N'감당할 수 있는 만큼만 맡게 하시고, 거절해야 할 때 거절할 수 있는 분별과 용기를 주옵소서. 봉사가 많아질수록 정작 주님과의 시간이 줄어드는 일이 없게 하옵소서.' + @P + N'주님, 아내가 교회를 섬기는') WHERE Id = 21;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 제가 아내에게 성령의',
 N'가장 가까운 사람에게 가장 인색해지기 쉬운 것이 사람의 마음입니다. 밖에서 베푸는 친절이 집 안에서도 그대로 흐르게 하시고, 식구들이 그 열매를 가장 먼저 맛보게 하옵소서.' + @P + N'주님, 제가 아내에게 성령의') WHERE Id = 22;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 저희 부부가 함께 결정해야',
 N'아내가 사람을 볼 때 겉모습에 속지 않게 하시고, 도움을 주는 척 다가오는 이들의 의도를 알아보게 하옵소서. 금전과 관련된 권유 앞에서 특별히 신중하게 하옵소서.' + @P + N'주님, 저희 부부가 함께 결정해야') WHERE Id = 23;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 아내가 그 부르심을',
 N'아내가 잘하는 일과 좋아하는 일이 무엇인지 스스로 돌아볼 여유를 주옵소서. 지금 서 있는 자리에서도 그것을 쓸 수 있음을 알게 하시고, 작은 일부터 시작하게 하옵소서.' + @P + N'주님, 아내가 그 부르심을') WHERE Id = 24;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 대화가 끊긴 데에는',
 N'말을 꺼냈다가 핀잔을 들은 기억 때문에 아내가 입을 닫지 않게 하옵소서. 저희 사이에 물어보아도 되는 것과 안 되는 것이 생기지 않게 하시고, 사소한 이야기도 나눌 수 있게 하옵소서.' + @P + N'주님, 대화가 끊긴 데에는') WHERE Id = 25;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 남 앞에서 아내를',
 N'말하지 않아도 알아주기를 바라다 서로 서운해지지 않게 하옵소서. 원하는 것을 부드럽게 말할 수 있게 하시고, 들은 쪽은 그 말을 흘려듣지 않게 하옵소서.' + @P + N'주님, 남 앞에서 아내를') WHERE Id = 26;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 먼저 사과하는 사람이',
 N'미안하다는 말이 형식이 되지 않게 하옵소서. 무엇이 잘못이었는지 알고 사과하게 하시고, 같은 잘못을 되풀이하지 않으려는 노력이 그 말 뒤에 따르게 하옵소서.' + @P + N'주님, 먼저 사과하는 사람이') WHERE Id = 27;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 아내가 저를 믿지 못하게',
 N'서로에게 숨기는 것이 없게 하옵소서. 돈과 사람과 시간에 관하여 정직하게 하시고, 말하지 않아도 되는 일과 반드시 말해야 하는 일을 분별하게 하옵소서.' + @P + N'주님, 아내가 저를 믿지 못하게') WHERE Id = 28;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 아내가 원하는 것은',
 N'서로를 향한 관심이 의무가 되지 않게 하옵소서. 오늘 아내의 표정이 어떤지 알아차리게 하시고, 좋은 일이 있을 때 가장 먼저 이야기하고 싶은 사람이 서로가 되게 하옵소서.' + @P + N'주님, 아내가 원하는 것은') WHERE Id = 29;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 그 자리를 여는 사람이',
 N'함께 기도한 것들에 주님이 응답하신 일을 기억하게 하옵소서. 그 기억이 저희의 믿음이 되게 하시고, 자녀들이 부모가 함께 기도하는 모습을 보고 자라게 하옵소서.' + @P + N'주님, 그 자리를 여는 사람이') WHERE Id = 30;

COMMIT TRAN;
PRINT '[BessingWife] Id 1~30 분량 보강 완료';
GO
