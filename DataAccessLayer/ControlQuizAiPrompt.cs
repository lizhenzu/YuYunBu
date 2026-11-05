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
