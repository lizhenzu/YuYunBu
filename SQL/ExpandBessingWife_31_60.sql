-- ============================================================
--  BessingWife 분량 보강 (Id 31~60)
-- ============================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;
DECLARE @P NVARCHAR(4) = CHAR(13)+CHAR(10)+CHAR(13)+CHAR(10);

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 다툰 뒤에 침묵으로',
 N'다투는 소리를 아이들이 듣게 하지 않게 하옵소서. 아이들 앞에서 서로를 깎아내리지 않게 하시고, 화해하는 모습까지 보여 주어 다툼이 끝이 아님을 배우게 하옵소서.' + @P + N'주님, 다툰 뒤에 침묵으로') WHERE Id = 31;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 아내를 저와 같아지게',
 N'서로의 속도가 다름도 받아들이게 하옵소서. 결정을 빨리 내리는 사람과 오래 생각하는 사람이 서로를 답답해하지 않게 하시고, 그 차이가 더 나은 결정을 만들게 하옵소서.' + @P + N'주님, 아내를 저와 같아지게') WHERE Id = 32;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 제가 한 약속을',
 N'결혼기념일과 서로의 생일을 흘려보내지 않게 하옵소서. 그날마다 지나온 세월을 함께 세어 보게 하시고, 처음 서약하던 그 마음을 다시 확인하게 하옵소서.' + @P + N'주님, 제가 한 약속을') WHERE Id = 33;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 저부터 그 시간을',
 N'함께 있는 시간에 각자 화면만 들여다보지 않게 하옵소서. 마주 앉아 눈을 보고 이야기하게 하시고, 그 시간이 짧아도 서로에게 가장 좋은 시간이 되게 하옵소서.' + @P + N'주님, 저부터 그 시간을') WHERE Id = 34;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 제가 그 일을 돕는',
 N'집안일을 두고 누가 더 많이 하는지 다투지 않게 하옵소서. 서로의 수고를 세어 보기보다 알아주게 하시고, 잘 정돈된 집에서 온 식구가 함께 누리게 하옵소서.' + @P + N'주님, 제가 그 일을 돕는') WHERE Id = 35;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 아내가 일터에서 돌아왔을',
 N'일터에서도 아내가 믿는 자답게 살게 하옵소서. 정직하지 않은 일에 손대지 않게 하시고, 아내의 성실함을 통해 주님이 드러나게 하옵소서.' + @P + N'주님, 아내가 일터에서 돌아왔을') WHERE Id = 36;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 그 시간을 제가 만들어',
 N'쉬는 시간에도 해야 할 일 목록이 머리에서 떠나지 않는 아내를 자유롭게 하옵소서. 쉴 때는 온전히 쉬게 하시고, 그 쉼이 다음 하루의 힘이 되게 하옵소서.' + @P + N'주님, 그 시간을 제가 만들어') WHERE Id = 37;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 자녀 양육이 아내 혼자의',
 N'아이의 말과 표정을 읽는 눈을 주시고, 아이가 무엇을 말하고 싶어 하는지 알아차리게 하옵소서. 훈계할 때와 품을 때를 분별하게 하시고, 감정으로 야단치지 않게 하옵소서.' + @P + N'주님, 자녀 양육이 아내 혼자의') WHERE Id = 38;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 자녀 문제로 저희 부부가',
 N'아이 때문에 마음이 무너지는 날에도 부부가 함께 기도의 자리로 나아가게 하옵소서. 아이를 향한 걱정을 주님께 올려 드리게 하시고, 주님이 그 아이의 진짜 아버지이심을 믿게 하옵소서.' + @P + N'주님, 자녀 문제로 저희 부부가') WHERE Id = 39;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 제가 그 사이에서 방관하지',
 N'명절과 가족이 모이는 자리가 아내에게 두려운 날이 되지 않게 하옵소서. 그 자리에서 오가는 말에 상처받지 않게 하시고, 아내도 누군가에게 상처 주는 말을 하지 않게 하옵소서.' + @P + N'주님, 제가 그 사이에서 방관하지') WHERE Id = 40;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 아내에게 가장 좋은 친구가',
 N'만나고 돌아온 뒤에 마음이 무거워지는 관계가 있다면 알아차리게 하옵소서. 사람을 만나는 일이 아내를 채우는 시간이 되게 하시고, 소모하는 시간이 되지 않게 하옵소서.' + @P + N'주님, 아내에게 가장 좋은 친구가') WHERE Id = 41;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 돈 문제로 저희 부부가',
 N'남과 견주느라 필요 이상으로 쓰지 않게 하시고, 두려움 때문에 지나치게 움켜쥐지도 않게 하옵소서. 저희에게 맡기신 것을 잘 관리하는 청지기가 되게 하옵소서.' + @P + N'주님, 돈 문제로 저희 부부가') WHERE Id = 42;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 아내가 무언가를 시작하려',
 N'배움이 아내의 짐이 되지 않게 하옵소서. 성과를 내야 한다는 부담 없이 즐길 수 있게 하시고, 중간에 멈추더라도 그 시간이 헛되지 않았음을 알게 하옵소서.' + @P + N'주님, 아내가 무언가를 시작하려') WHERE Id = 43;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 제가 낸 화가',
 N'제 안에 쌓인 것을 제때 주님께 풀어 놓게 하옵소서. 밖에서 받은 상처를 안에서 터뜨리지 않게 하시고, 힘들면 힘들다고 말할 수 있는 정직함을 주옵소서.' + @P + N'주님, 제가 낸 화가') WHERE Id = 44;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 제 순결이 아내를',
 N'다른 사람과 아내를 비교하는 생각이 마음에 들어오는 순간 곧바로 끊어 내게 하옵소서. 아내에게 감사할 이유를 그때마다 떠올리게 하시고, 마음을 아내에게 돌리게 하옵소서.' + @P + N'주님, 제 순결이 아내를') WHERE Id = 45;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 제 힘으로 되는 일이',
 N'제가 아내에게 바라는 것을 저 자신에게 먼저 요구하게 하옵소서. 아내에게 하라고 말하는 것을 제가 먼저 하게 하시고, 그 순서를 뒤바꾸지 않게 하옵소서.' + @P + N'주님, 제 힘으로 되는 일이') WHERE Id = 46;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 아내가 저를 통해',
 N'가정을 이끄는 일에 아내의 지혜를 함께 쓰게 하옵소서. 혼자 결정하고 통보하지 않게 하시고, 아내의 생각을 묻고 함께 세워 가는 가장이 되게 하옵소서.' + @P + N'주님, 아내가 저를 통해') WHERE Id = 47;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 이 기도가 하루로',
 N'아내의 이름을 부르며 기도할 때 제 마음이 먼저 부드러워지게 하옵소서. 기도하는 동안 아내를 향한 미움이 녹게 하시고, 아내를 주님의 눈으로 보게 하옵소서.' + @P + N'주님, 이 기도가 하루로') WHERE Id = 48;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 아내가 늦는 날',
 N'급한 마음으로 서두르다 사고를 만나지 않게 하옵소서. 늦더라도 안전을 먼저 택하게 하시고, 아내가 다니는 모든 길에 주님의 사자를 보내 주옵소서.' + @P + N'주님, 아내가 늦는 날') WHERE Id = 49;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 이 싸움에서 제가',
 N'아내가 두려움에 사로잡히는 순간 곧바로 주님의 이름을 부르게 하옵소서. 말씀 한 구절이라도 마음에 담아 두게 하시고, 그 말씀으로 거짓을 물리치게 하옵소서.' + @P + N'주님, 이 싸움에서 제가') WHERE Id = 50;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 아내의 담대함을 제가',
 N'거절해야 할 때 거절하게 하옵소서. 모두를 만족시키려다 자신을 잃지 않게 하시고, 사람의 기분보다 주님의 뜻을 먼저 살피게 하옵소서.' + @P + N'주님, 아내의 담대함을 제가') WHERE Id = 51;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 아내의 꿈을 제가',
 N'꿈을 이야기할 때 비웃음을 사지 않게 하옵소서. 아내가 마음속 소원을 말할 수 있는 안전한 자리가 저희 가정 안에 있게 하시고, 그 말을 진지하게 들어주는 식구들이 되게 하옵소서.' + @P + N'주님, 아내의 꿈을 제가') WHERE Id = 52;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 아내가 결정을 앞두고',
 N'결정을 내린 뒤에 지난 선택을 되씹으며 자신을 괴롭히지 않게 하옵소서. 최선을 다해 구하고 결정했다면 그 길에서 주님을 신뢰하며 걸어가게 하옵소서.' + @P + N'주님, 아내가 결정을 앞두고') WHERE Id = 53;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 집의 공기를 만드는',
 N'저희 집에 찾아오는 이들이 쉼을 얻게 하옵소서. 아내가 손님을 맞는 일에 지치지 않게 하시고, 나누는 일이 부담이 아니라 기쁨이 되게 하옵소서.' + @P + N'주님, 집의 공기를 만드는') WHERE Id = 54;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 아내의 노년에 제가',
 N'나이가 들수록 마음이 굳어지지 않게 하옵소서. 새로운 것을 배우는 즐거움을 잃지 않게 하시고, 젊은 사람들의 말을 들을 줄 아는 넉넉함을 주옵소서.' + @P + N'주님, 아내의 노년에 제가') WHERE Id = 55;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 아내에게 끝까지 고마운',
 N'저희 중 하나가 먼저 약해질 때 남은 사람에게 힘을 주옵소서. 그 시간이 두려움이 아니라 사랑을 확인하는 시간이 되게 하시고, 마지막까지 서로의 손을 놓지 않게 하옵소서.' + @P + N'주님, 아내에게 끝까지 고마운') WHERE Id = 56;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 그 감사를 마음에만',
 N'아내가 저를 위해 기도해 온 시간을 기억하게 하옵소서. 제가 알지 못하는 자리에서 저를 위해 눈물로 구했을 그 기도를 주님이 갚아 주옵소서.' + @P + N'주님, 그 감사를 마음에만') WHERE Id = 57;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 그때에 제가 도망하지',
 N'어려운 소식을 들었을 때 아내가 혼자 감당하지 않게 하옵소서. 곧바로 저에게 말할 수 있게 하시고, 저희가 함께 무릎 꿇는 것으로 그 일을 시작하게 하옵소서.' + @P + N'주님, 그때에 제가 도망하지') WHERE Id = 58;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 하루의 끝에 아내가',
 N'하루 중에 잠깐씩 주님을 떠올리게 하옵소서. 일하다가도 길을 걷다가도 짧은 기도를 올려 드리게 하시고, 그 순간마다 주님이 함께 계심을 느끼게 하옵소서.' + @P + N'주님, 하루의 끝에 아내가') WHERE Id = 59;

UPDATE dbo.BessingWife SET PrayText = REPLACE(PrayText, N'주님, 이 축복이 제 입에서',
 N'아내의 자녀들이 일어나 아내를 감사하게 하옵소서. 아내의 수고가 열매로 돌아오게 하시고, 아내가 심은 것을 아내의 눈으로 거두어 보게 하옵소서.' + @P + N'주님, 이 축복이 제 입에서') WHERE Id = 60;

COMMIT TRAN;
PRINT '[BessingWife] Id 31~60 분량 보강 완료';
GO
