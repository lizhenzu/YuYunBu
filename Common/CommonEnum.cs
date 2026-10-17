using BulletinBoard.BusinessLayer;
using BulletinBoard.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace BulletinBoard.Common
{
    public enum Category
    {
        GongGwa,
        People,
        BibileStudy,
        Related,
        RelatedBible,
        Geomorphological,
        BackGround,
        Adventure,
        Encourage,
        Value,
        Student,
        Teacher,
        BibleExplanation,
        StoryTellingBook,
        StudentAll,
        All,
    };

    public enum ProductDisplayOpton
    {
        display,
        challengeMissionBible,
        newComer,
        managePrice,
        birth,
        Special,
        FamilyTeam1K,
        FamilyTeam2K,
        FamilyTeam3K,
        FamilyTeam4K,
        FamilyTeam5K,
        none
    };

    /// <summary>
    /// 가족팀 합산 선물 정책
    ///   합산 1000점 → FamilyTeam2K (2만원 미만)
    ///   합산 2000점 → 박수(선물 없음)
    ///   합산 3000점 → FamilyTeam3K (3만원 미만)
    ///   합산 4000점 → FamilyTeam4K (4만원 미만)
    ///   합산 5000점 → FamilyTeam5K (5만원 미만)
    /// </summary>
    public static class ProductDisplayOptonHelper
    {
        /// <summary>마일스톤 점수 → 선물 목록 옵션. 선물이 없는 점수면 null.</summary>
        public static ProductDisplayOpton? FamilyTeamOption(int threshold) => threshold switch
        {
            1000 => ProductDisplayOpton.FamilyTeam2K,
            3000 => ProductDisplayOpton.FamilyTeam3K,
            4000 => ProductDisplayOpton.FamilyTeam4K,
            5000 => ProductDisplayOpton.FamilyTeam5K,
            _    => null
        };

        /// <summary>옵션 → 가격 상한(미만). 가족팀 옵션이 아니면 null.</summary>
        public static int? FamilyTeamPriceLimit(ProductDisplayOpton option) => option switch
        {
            ProductDisplayOpton.FamilyTeam2K => 20000,
            ProductDisplayOpton.FamilyTeam3K => 30000,
            ProductDisplayOpton.FamilyTeam4K => 40000,
            ProductDisplayOpton.FamilyTeam5K => 50000,
            _ => null
        };
    }

    public enum RankQueryOption
    {
        StudentOnly,
        TeacherOnly,
        All,
        OnClass,
        None,
        NoPhoto,
        UnClassify,
        maxClickDate
    };
    public enum BilbeProcessOption
    {
        None,
        TotalProgressRate,
        ReadDate
    };

    public enum VideoKindOption
    {
        Child,
        Adult,
        KorEnglish,
        Life,
        CorrectExpression,
        Practical
    };


    public enum HumanResourceCategory
    {
        HROnly,
        HRForSendMessage
    };


    public enum Couple
    {
        Husband,
        Wife
    };

    public enum DateKind
    {
        WorkDT,
        TodayDT,
        WeekNaviDT,
        TotalElapseDays
    };

    public enum HttpContextSessionCategory
    {
        DeptCode,
        DeptNM,
        UserFullName,
        UserName,
        UserId,
        ImpersonateUserId,
        LangCD,
        PaesForView,
        Notice_1_Collapse,
        BibleBookOverview,
        OneLineText
    }; 
    
    public enum CommonMetaDataCategory
    {
        ChurchName,        
        DepartmentName
    };

    public enum BoardCategory
    {
        Anonymous,
        HelpDesk 
    };
    //
    
    public enum QuizCategory
    {
        Broad
    };
    public enum MissionKinds
    {
        TodayWordToRead,
        JuManNa,
        IndexByID

    }; 
    
    public enum ControllerName
    {
        WeeklyReport
    };

    public enum MVCInfomation
    {
        ActionSource,
        ControllerSource
    };


    public enum CookieKey
    {
        MissionMainBoard_HideDay
    };




    public enum PopMessageReadMehod
    {
        CheckMyNessMessage,
        ReadMyNewMessage,
        ReadMyAllMessage,
        ReadAllMessage,
        ReadISentAllMessage,
        MarkMyNewMessage,
    };

    public enum HTTPSessionKey
    {
        WeeklyWordToReadID,
    };

}
