using System;
using System.Collections.Generic;

namespace BulletinBoard.Models.Jubo
{
    public class SundaySchoolJubo
    {
        public int Id { get; set; }
        public DateTime JuboDate { get; set; }
        public string? Season { get; set; }
        public string Title { get; set; } = string.Empty;
        public string TemplateType { get; set; } = "Normal";
        public bool IsPublished { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public List<SundaySchoolJuboBlock> Blocks { get; set; } = new();
    }

    public class SundaySchoolJuboBlock
    {
        public int Id { get; set; }
        public int JuboId { get; set; }
        public string BlockType { get; set; } = string.Empty;
        public string? BlockTitle { get; set; }
        public string? BlockContent { get; set; }
        public int SortOrder { get; set; }
        public bool IsVisible { get; set; } = true;
        public string? LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }

    public class SundaySchoolJuboSnapshot
    {
        public int Id { get; set; }
        public int JuboId { get; set; }
        public string? SnapshotJson { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class AttendanceCheck2026
    {
        public int Id { get; set; }
        public DateTime JuboDate { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int PresentCount { get; set; }
        public int NewComerCount { get; set; }
        public string? Note { get; set; }
        public string? RecordedBy { get; set; }
        public DateTime RecordedAt { get; set; }
        public bool IsLocked { get; set; }
    }

    public class AttendanceNewComerModel
    {
        public int Id { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? Age { get; set; }
        public string? Grade { get; set; }
        public string? ParentName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Note { get; set; }
        public string? RecordedBy { get; set; }
        public DateTime RecordedAt { get; set; }
    }

    public static class JuboBlockTypes
    {
        public const string WorshipOrder            = "WorshipOrder";
        public const string NextWeekPrayerTeam      = "NextWeekPrayerTeam";      // 다음주 대표기도반
        public const string NextWeekStreetEvangelism= "NextWeekStreetEvangelism"; // 다음주 노방전도반
        public const string ScriptureText           = "ScriptureText";
        public const string MemoryVerse             = "MemoryVerse";
        public const string MissionGuide            = "MissionGuide";
        public const string EventAnnouncements      = "EventAnnouncements";
        public const string Birthday                = "Birthday";
        public const string ParentNotice            = "ParentNotice";
        public const string GiftNotice              = "GiftNotice";

        public static readonly string[] RequiredTypes =
            { WorshipOrder, ScriptureText, MemoryVerse, MissionGuide, EventAnnouncements };

        public static readonly string[] OptionalTypes =
            { NextWeekPrayerTeam, NextWeekStreetEvangelism, Birthday, ParentNotice, GiftNotice };

        public static readonly string[] AllTypes =
        {
            WorshipOrder, NextWeekPrayerTeam, NextWeekStreetEvangelism,
            ScriptureText, MemoryVerse, MissionGuide, EventAnnouncements,
            Birthday, ParentNotice, GiftNotice
        };

        public static string GetKoreanName(string blockType) => blockType switch
        {
            WorshipOrder             => "예배 순서",
            NextWeekPrayerTeam       => "다음주 대표기도반",
            NextWeekStreetEvangelism => "다음주 노방전도반",
            ScriptureText            => "본문 말씀",
            MemoryVerse              => "암송 말씀",
            MissionGuide             => "사명 안내",
            EventAnnouncements       => "행사 및 공지",
            Birthday                 => "생일 축하",
            ParentNotice             => "학부모 공지",
            GiftNotice               => "선물 안내",
            _                        => blockType
        };

        public static string GetEmoji(string blockType) => blockType switch
        {
            WorshipOrder             => "🙏",
            NextWeekPrayerTeam       => "🙌",
            NextWeekStreetEvangelism => "🚶",
            ScriptureText            => "📖",
            MemoryVerse              => "✨",
            MissionGuide             => "🚀",
            EventAnnouncements       => "📢",
            Birthday                 => "🎂",
            ParentNotice             => "👨‍👩‍👧‍👦",
            GiftNotice               => "🎁",
            _                        => "📌"
        };

        public static string GetDefaultTitle(string blockType) => blockType switch
        {
            WorshipOrder             => "오늘의 예배 순서",
            NextWeekPrayerTeam       => "다음주 대표기도반",
            NextWeekStreetEvangelism => "다음주 노방전도반",
            ScriptureText            => "이번 주 말씀",
            MemoryVerse              => "암송 말씀",
            MissionGuide             => "이번 주 사명",
            EventAnnouncements       => "공지사항",
            Birthday                 => "이번 주 생일 친구들 🎂",
            ParentNotice             => "학부모님께 알립니다 💌",
            GiftNotice               => "선물 안내 🎁",
            _                        => blockType
        };

        public static int GetDefaultSortOrder(string blockType) => blockType switch
        {
            WorshipOrder             => 10,
            NextWeekPrayerTeam       => 15,
            NextWeekStreetEvangelism => 16,
            ScriptureText            => 20,
            MemoryVerse              => 30,
            MissionGuide             => 40,
            EventAnnouncements       => 50,
            Birthday                 => 60,
            ParentNotice             => 70,
            GiftNotice               => 80,
            _                        => 99
        };

        public static string GetColorClass(string blockType) => blockType switch
        {
            WorshipOrder             => "block-sky",
            NextWeekPrayerTeam       => "block-lavender",
            NextWeekStreetEvangelism => "block-peach",
            ScriptureText            => "block-mint",
            MemoryVerse              => "block-yellow",
            MissionGuide             => "block-orange",
            EventAnnouncements       => "block-pink",
            Birthday                 => "block-lavender",
            ParentNotice             => "block-peach",
            GiftNotice               => "block-green",
            _                        => "block-sky"
        };
    }

    public static class JuboTemplateTypes
    {
        public const string Normal = "Normal";
        public const string PalmSunday = "PalmSunday";
        public const string Easter = "Easter";

        public static string GetKoreanName(string type) => type switch
        {
            Normal => "일반",
            PalmSunday => "종려주일",
            Easter => "부활절",
            _ => type
        };

        public static string GetEmoji(string type) => type switch
        {
            Normal => "⛪",
            PalmSunday => "🌿",
            Easter => "🥚",
            _ => "⛪"
        };

        public static string GetDescription(string type) => type switch
        {
            Normal => "일반 주일 주보 (파란색 테마)",
            PalmSunday => "종려주일 특별 주보 (보라/초록 테마)",
            Easter => "부활절 특별 주보 (노랑/분홍 테마)",
            _ => type
        };
    }
}
