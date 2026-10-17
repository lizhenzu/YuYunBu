# -*- coding: utf-8 -*-

import re
import os
import json
import time
import requests
from bs4 import BeautifulSoup

# ──────────────────────────────────────────────────────────
# 설정
# ──────────────────────────────────────────────────────────

BIBLE_BASE = "http://www.holybible.or.kr/B_HDB/cgi/bibleftxt.php"
API_URL    = "https://www.jamsilkids.com/api/PrayFreind/ExecuteSql"
TEMP_FILE  = "bible_temp.sql"

VR         = "HDB"
CV         = 99
DELAY_SEC  = 4.0

VL_START = 19
VL_END   = 19

BIBLE_HEADERS = {
    "User-Agent": (
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
        "AppleWebKit/537.36 (KHTML, like Gecko) "
        "Chrome/124.0.0.0 Safari/537.36"
    ),
    "Referer": "http://www.holybible.or.kr/",
    "Accept": "text/html,application/xhtml+xml",
    "Accept-Language": "ko-KR,ko;q=0.9",
}

BOOK_NAMES = [
    "창세기","출애굽기","레위기","민수기","신명기","여호수아","사사기","룻기",
    "사무엘상","사무엘하","열왕기상","열왕기하","역대상","역대하","에스라",
    "느헤미야","에스더","욥기","시편","잠언","전도서","아가","이사야","예레미야",
    "예레미야애가","에스겔","다니엘","호세아","요엘","아모스","오바댜","요나",
    "미가","나훔","하박국","스바냐","학개","스가랴","말라기",
    "마태복음","마가복음","누가복음","요한복음","사도행전","로마서",
    "고린도전서","고린도후서","갈라디아서","에베소서","빌립보서","골로새서",
    "데살로니가전서","데살로니가후서","디모데전서","디모데후서","디도서","빌레몬서",
    "히브리서","야고보서","베드로전서","베드로후서","요한일서","요한이서",
    "요한삼서","유다서","요한계시록",
]

# ──────────────────────────────────────────────────────────
# HTML 요청
# ──────────────────────────────────────────────────────────

def fetch_html(vl: int, cn: int) -> str:
    url = f"{BIBLE_BASE}?VR={VR}&VL={vl}&CN={cn}&CV={CV}"
    r = requests.get(url, headers=BIBLE_HEADERS, timeout=15)
    r.encoding = "euc-kr"
    r.raise_for_status()
    return r.text


# ──────────────────────────────────────────────────────────
# 메타 파싱
# ──────────────────────────────────────────────────────────

def parse_meta(soup) -> dict:
    meta = {"book": None, "chapter": None, "version_name": None}
    for td in soup.find_all("td", class_="tkb"):
        text = td.get_text(strip=True)
        m = re.match(r"^(.+?)\s+(\d{1,3})편\s*\[(.+?)\]", text)
        if m:
            meta["book"]         = m.group(1).strip()
            meta["chapter"]      = int(m.group(2))
            meta["version_name"] = m.group(3).strip()
            break
    return meta


# ──────────────────────────────────────────────────────────
# 절 파싱
# ──────────────────────────────────────────────────────────

def clean_text(font_tag) -> str:
    """
    <font class="tk4l"> 안의 텍스트 추출.

    문제 원인:
      get_text(strip=True) 는 각 태그 안 텍스트 노드의 앞뒤 공백을 제거함.
      예) "도움으로 <a>남자</a>아이를"
          → "도움으로 " 의 뒤 공백이 strip 되어 → "도움으로남자아이를" (오류)

    해결:
      strip=False 로 공백 보존 후, 연속 공백만 단일 공백으로 정리.
    """
    text = font_tag.get_text(separator="", strip=False)  # ← strip=False 핵심
    text = re.sub(r"\s+", " ", text)                     # 연속 공백 → 단일 공백
    return text.strip()                                  # 양 끝 공백 제거


def parse_verses(html: str) -> dict:
    soup   = BeautifulSoup(html, "html.parser")
    meta   = parse_meta(soup)
    verses = []

    for cell in soup.find_all("td", class_="tk4br"):
        ol = cell.find("ol")
        if not ol:
            continue
        start = int(ol.get("start", 1))
        for offset, font in enumerate(ol.find_all("font", class_="tk4l")):
            text = clean_text(font)
            if text:
                verses.append({"verse": start + offset, "text": text})

    meta["verses"] = verses
    return meta


# ──────────────────────────────────────────────────────────
# SQL
# ──────────────────────────────────────────────────────────

def esc(text: str) -> str:
    return text.replace("'", "''")


def make_upsert(bt: str, jang: int, zul: int, data: str) -> str:
    return (
        f"IF EXISTS ("
        f" SELECT 1 FROM [dbo].[eBibleHD]"
        f" WHERE [BibleText]=N'{esc(bt)}'"
        f" AND [Jang]={jang}"
        f" AND [zul]={zul}"
        f") "
        f"UPDATE [dbo].[eBibleHD] "
        f"SET [BibleData]=N'{esc(data)}',"
        f" [updateDT]=GETDATE() "
        f"WHERE [BibleText]=N'{esc(bt)}'"
        f" AND [Jang]={jang}"
        f" AND [zul]={zul} "
        f"ELSE "
        f"INSERT INTO [dbo].[eBibleHD] "
        f"([BibleText],[Jang],[zul],[BibleData],[InsertDT],[updateDT]) "
        f"VALUES "
        f"(N'{esc(bt)}',{jang},{zul},N'{esc(data)}',GETDATE(),GETDATE())"
    )


def build_transaction(upserts: list) -> str:
    parts = ["SET XACT_ABORT ON", "BEGIN TRANSACTION", ""]
    for u in upserts:
        parts.append(u)
        parts.append("")
    parts.append("COMMIT TRANSACTION")
    return "\n".join(parts)


# ──────────────────────────────────────────────────────────
# API 전송
# ──────────────────────────────────────────────────────────

def send_sql(sql: str):
    try:
        resp = requests.post(
            API_URL,
            headers={"Content-Type": "application/json"},
            data=json.dumps({"sql": sql}),
            timeout=30,
        )
        try:
            msg = resp.json().get("message", resp.text)
        except Exception:
            msg = resp.text
        if resp.status_code == 200:
            return True, msg
        return False, f"HTTP {resp.status_code}: {msg}"
    except Exception as e:
        return False, str(e)


# ──────────────────────────────────────────────────────────
# 장 처리
# ──────────────────────────────────────────────────────────

def process_chapter(vl: int, cn: int):
    url = f"{BIBLE_BASE}?VR={VR}&VL={vl}&CN={cn}&CV={CV}"
    print(f"\n크롤링: {url}")

    try:
        html   = fetch_html(vl, cn)
        result = parse_verses(html)
    except Exception as e:
        print(f"❌ 요청 실패: {e}")
        return False

    book    = result.get("book")
    chapter = result.get("chapter")
    verses  = result.get("verses", [])

    if not book or not chapter:
        print("⚠ 장 정보 없음")
        return False

    if not verses:
        print("⚠ 절 정보 없음")
        return False

    print(f"✔ {book} {chapter}편 ({len(verses)}절)")

    upserts  = [make_upsert(book, chapter, v["verse"], v["text"]) for v in verses]
    full_sql = build_transaction(upserts)
    #print('full_sql : ' , full_sql)
    ok, msg = send_sql(full_sql)

    if ok:
        print("✅ 저장 완료")
        return True

    print(f"❌ 저장 실패: {msg}")
    return False


# ──────────────────────────────────────────────────────────
# 메인
# ──────────────────────────────────────────────────────────

def main():
    for vl in range(VL_START, VL_END + 1):
        print(f"\n{'='*60}")
        print(f"{vl}권 처리 시작")
        print(f"{'='*60}")

        cn = 1
        while True:
            ok = process_chapter(vl, cn)
            if not ok:
                print(f"끝: VL={vl} CN={cn}")
                break
            cn += 1
            time.sleep(DELAY_SEC)


if __name__ == "__main__":
    main()
