using System;

namespace StockBulletinBoard.Models
{
    public class DailyEnglish
    {
        public int Id { get; set; }
        public string EnglishText { get; set; }
        public string KoreanText { get; set; }
        public string Pronunciation { get; set; }
        public string Description { get; set; }
        public string Example { get; set; }
        public string ExampleKorean { get; set; }   // ✅ 예문 한글 번역
        public string VocabNote { get; set; }        // ✅ 핵심 단어/숙어 설명
        public int IsRead { get; set; }   // ✅ 읽은 횟수 (0=미읽음, 1+=읽음 횟수)
        public DateTime CreatedAt { get; set; }
    }
}