using BulletinBoard.DataAccessLayer;
using BulletinBoard.Helpers;
using System;
using System.Collections.Generic;
using System.Data;

namespace BulletinBoard.BusinessLayer
{
    public class QuizAiPromptInfo
    {
        public string PromptText { get; set; } = "";
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; } = "";

        /// <summary>false 면 DB 에서 읽지 못해 코드의 기본 문구를 쓰는 중이다</summary>
        public bool FromDb { get; set; }
    }

    /// <summary>화면에서 보고 복사만 하는 프람프트 한 개 (dbo.AiPromptLibrary)</summary>
    public class AiPromptLibraryItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string PromptText { get; set; } = "";
    }

    public class ManageQuizAiPrompt
    {
        /// <summary>프람프트 모음을 읽는다. 표가 없거나 오류면 빈 목록</summary>
        public List<AiPromptLibraryItem> GetLibrary()
        {
            var list = new List<AiPromptLibraryItem>();
            DataSet ds = new ControlQuizAiPrompt().fetchLibrary();
            if (ds.Tables.Count == 0) return list;

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                list.Add(new AiPromptLibraryItem
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Title = row["Title"] as string ?? "",
                    PromptText = (row["PromptText"] as string ?? "").Replace("\r\n", "\n"),
                });
            }
            return list;
        }

        /// <summary>DB 문구를 읽는다. 표가 없거나 연결 오류 · 빈 값이면 코드의 기본 문구를 돌려준다</summary>
        public QuizAiPromptInfo Get()
        {
            var fallback = new QuizAiPromptInfo { PromptText = QuizAiPromptHelper.Normalize(QuizAiPromptHelper.DefaultTemplate) };

            DataSet ds = new ControlQuizAiPrompt().fetchPrompt();   // 오류면 표 없는 DataSet 이 온다
            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0) return fallback;

            DataRow row = ds.Tables[0].Rows[0];
            string text = QuizAiPromptHelper.Normalize(row["PromptText"] as string);
            if (text.Length == 0) return fallback;

            return new QuizAiPromptInfo
            {
                PromptText = text,
                UpdatedAt = row["UpdatedAt"] as DateTime?,
                UpdatedBy = row["UpdatedBy"] as string ?? "",
                FromDb = true
            };
        }

        public string GetTemplate() => Get().PromptText;

        public void Save(string promptText, string updatedBy)
        {
            new ControlQuizAiPrompt().savePrompt(QuizAiPromptHelper.Normalize(promptText), DateHelper.GetKoreaNow(), updatedBy);
        }
    }
}
