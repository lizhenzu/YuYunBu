using BulletinBoard.BusinessLayer;
using BulletinBoard.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace BulletinBoard.Common
{
    public static class ConstatsCommon
    {
        public static bool addPoint = true;
        public static string MainKeyTitle = "도전!!! 성경말씀";
        public static string BibleNote = "성경노트";
        public static string ReadBibleAllTitle = "성경통독Board";
        public static string ReadBibleAllTitleButton = "도전!!! 성경통독";
        public static string MissionBibleWithparent = "부모님과 함께하는 미션북";
        public static string HolyWordAndRead = "감사가 자라는 말씀과 일기노트";
        public static string ToHome = "홈으로";
        
        public static string myDoToRi = "나의 도토리!!!";

        public static string AnonymousBoardName = "공과개선방(톡!톡!)";

        public static int popupAutoCloseSeconds = 7;
        public static int attendancePoint = 1;
        public static DateTime VBSParticipationStartDate = new DateTime(2026, 7, 31);

        public static string TalkWithFriends = "1:1 이야기 공간";
        public static string AIExperience = "인공지능(GPT) 체험공간";
        public static int TodayReadCountLimitMax = 500;
        public static int TodayReadCountLimitMin = 50;
    
        public static string representFaceImage = "/images/icon/people/people_01.webp";
        public static string HelpDeskHome = "개선사항접수";
        public static int timeDifference16 = 16;  //16 or 0 
        public static int birthDayPresentMax = 5000;  //16 or 0 
        public static (string, int)[] missionKinds = {
            ("말씀", 29),
            ("퀴즈", 12),
            ("기도", 13),
            ("암송", 12)
        };

    }



    public static class ConstatsColorCommon
    {
        public static string HeaderColor1 = "#112233";
        public static string HeaderColor2 = "#006633";
    }
}
