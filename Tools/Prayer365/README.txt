입력 파일 형식 (UTF-8). 항목 사이는 ===== 한 줄.
BODY: 아래는 빈 줄 = 문단 나눔, 같은 문단 안의 줄바꿈은 공백으로 합쳐짐.
FIX 줄은 없어도 됨 (교정 기록용, 여러 줄 가능). VERSE_REF / VERSE도 비워 둘 수 있음.

MD: 0101
TITLE: 제목
VERSE_REF: 잠 22:6
VERSE: 구절 본문
FIX: 원문 => 교정
BODY:
첫 문단 첫 줄
첫 문단 둘째 줄

둘째 문단
=====
MD: 0102
...

실행: powershell -File Tools\Prayer365\Prayer365_ToSql.ps1 -InputFile 입력.txt
결과: SQL\Prayer365_시작-끝_YYYYMMDD.sql (BOM 없음, CRLF, 같은 날짜는 덮어씀)
