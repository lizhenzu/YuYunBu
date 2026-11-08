using BulletinBoard.Common;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace BulletinBoard.DataAccessLayer
{
    /// <summary>AI 버튼이 복사하는 공용 프람프트 (dbo.QuizAiPrompt, 한 줄만 쓴다)</summary>
    public class ControlQuizAiPrompt
    {
        public DataSet fetchPrompt()
        {
            return DataAccessModule.commonQuery("select PromptText, UpdatedAt, UpdatedBy from [dbo].[QuizAiPrompt] where Id = 1");
        }

        /// <summary>프람프트 모음 (dbo.AiPromptLibrary) — 표가 없으면 표 없는 DataSet 이 온다</summary>
        public DataSet fetchLibrary()
        {
            return DataAccessModule.commonQuery("select Id, Title, PromptText from [dbo].[AiPromptLibrary] order by SortNo, Id");
        }

        /// <summary>프람프트 모음 한 개를 고친다. 고친 줄 수를 돌려준다 (없는 번호면 0)</summary>
        public int updateLibrary(int id, string title, string promptText, DateTime updatedAt)
        {
            const string sql = "UPDATE [dbo].[AiPromptLibrary] SET Title = @Title, PromptText = @PromptText, UpdatedAt = @UpdatedAt WHERE Id = @Id";
            return DataAccessModule.executeNonQuery(sql, new List<SqlParameter>
            {
                new SqlParameter("@Id", SqlDbType.Int) { Value = id },
                new SqlParameter("@Title", SqlDbType.NVarChar, 200) { Value = title },
                new SqlParameter("@PromptText", SqlDbType.NVarChar, -1) { Value = promptText },
                new SqlParameter("@UpdatedAt", SqlDbType.DateTime2) { Value = updatedAt }
            });
        }

        public void savePrompt(string promptText, DateTime updatedAt, string updatedBy)
        {
            const string sql = @"
IF EXISTS (SELECT 1 FROM [dbo].[QuizAiPrompt] WHERE Id = 1)
    UPDATE [dbo].[QuizAiPrompt] SET PromptText = @PromptText, UpdatedAt = @UpdatedAt, UpdatedBy = @UpdatedBy WHERE Id = 1;
ELSE
    INSERT INTO [dbo].[QuizAiPrompt] (Id, PromptText, UpdatedAt, UpdatedBy) VALUES (1, @PromptText, @UpdatedAt, @UpdatedBy);";

            DataAccessModule.executeNonQuery(sql, new List<SqlParameter>
            {
                new SqlParameter("@PromptText", SqlDbType.NVarChar, -1) { Value = promptText },
                new SqlParameter("@UpdatedAt", SqlDbType.DateTime2) { Value = updatedAt },
                new SqlParameter("@UpdatedBy", SqlDbType.NVarChar, 256) { Value = updatedBy }
            });
        }
    }
}
