using System.Collections.Generic;
using StockBulletinBoard.Models;

namespace StockBulletinBoard.Services
{
    public interface IDailyEnglishService
    {
        DailyEnglish GetNext();           // 가장 적게 읽힌 문장
        void IncrementIsRead(int id);     // 화면 표시 시 +1
        void MarkAsRead(int id);          // 읽음 버튼 클릭 시 추가 +1
        void Reset();

        List<DailyEnglish> GetAll();
        DailyEnglish GetById(int id);
        void Insert(DailyEnglish item);
        void Update(DailyEnglish item);
        void Delete(int id);

        // ✅ 이전/다음 ID 탐색
        int? GetPrevId(int currentId);
        int? GetNextId(int currentId);
        // ✅ 현재 문장의 순번 (1-based), 전체 건수, 미읽음 건수
        (int position, int total, int unread) GetPositionAndTotal(int currentId);

        // ✅ YouTube 영상 관리
        List<DailyEnglishVideo> GetAllVideos();
        DailyEnglishVideo       GetVideoById(int id);
        void InsertVideo(string youTubeId, string title);
        void DeleteVideo(int id);
    }
}