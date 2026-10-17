using BulletinBoard.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlCommonCookie
    {

        public DataSet fetchUserCookieSettings(string UserId, string cookieName)
        {
            StringBuilder sb = new StringBuilder();


            DataAccessModule.executeNonQuery("dbo.DeleteExpiredUserSettingsCookies");


            //  sb.AppendFormat(" select top 100 ROW_NUMBER() OVER(ORDER BY  Jang, zul )  sequenceNo ,");
            sb.AppendFormat($" select          isnull([Id]     , '') UserId      ,");
            sb.AppendFormat($"                 isnull([cookieName] , '') cookieName  ,");
            sb.AppendFormat($"                 isnull([CookieValue], '') CookieValue ,");
            sb.AppendFormat($"                 isnull([CookieDays] , 0 ) CookieDays   ");
            sb.AppendFormat($" from   [dbo].[UserSettings_cookieName] ");
            sb.AppendFormat($" where  Id         = '{UserId}' ");
            sb.AppendFormat($" and    cookieName = '{cookieName}' ");

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }




        public void UpsertUserCookieSettings(
    string userId,
    string? cookieName = null,
    string? cookieValue = null,
    int? cookieDays = null)
        {
            if (cookieName == null && cookieValue == null && cookieDays == null)
                throw new ArgumentException("cookieName, cookieValue, cookieDays 중 최소 하나는 지정해야 합니다.");

            // 보간($) 제거, 상수로 안전하게 유지
            const string sql = @"
MERGE [dbo].[UserSettings_cookieName] AS target
USING (VALUES (@Id, @CookieName, @CookieValue, @CookieDays))
       AS source ([Id], [cookieName], [cookieValue], [cookieDays])
    ON target.[Id] = source.[Id]
WHEN MATCHED THEN
    UPDATE SET
        cookieName  = COALESCE(source.[cookieName], target.[cookieName]),
        cookieValue = COALESCE(source.[cookieValue], target.[cookieValue]),
        cookieDays  = COALESCE(source.[cookieDays], target.[cookieDays]),
        updateDT    = DATEADD(HOUR, @TimeOffset, GETDATE())
WHEN NOT MATCHED THEN
    INSERT ([Id], [cookieName], [cookieValue], [cookieDays], [InsertDT], [updateDT])
    VALUES (source.[Id], source.[cookieName], source.[cookieValue], source.[cookieDays],
            DATEADD(HOUR, @TimeOffset, GETDATE()),
            DATEADD(HOUR, @TimeOffset, GETDATE()));";

            var parameters = new List<SqlParameter>
    {
        new SqlParameter("@Id", SqlDbType.NVarChar, 450) { Value = userId },
        new SqlParameter("@CookieName", SqlDbType.NVarChar, 200) { Value = (object?)cookieName ?? DBNull.Value },
        new SqlParameter("@CookieValue", SqlDbType.NVarChar, 200) { Value = (object?)cookieValue ?? DBNull.Value },
        new SqlParameter("@CookieDays", SqlDbType.Int) { Value = (object?)cookieDays ?? DBNull.Value },
        // 상수 보간 대신 파라미터 사용
        new SqlParameter("@TimeOffset", SqlDbType.Int) { Value = ConstatsCommon.timeDifference16 }
    };

            DataAccessModule.executeNonQuery(sql, parameters);
        }


        //    public void UpsertUserC1ookieSettings(string UserId, string? cookieName = null, string? cookieValue = null, int? cookieDays = null)
        //    {
        //        if (cookieName == null && cookieValue == null && cookieDays == null)
        //            throw new ArgumentException("cookieName, cookieValue, cookieDays 중 최소 하나는 지정해야 합니다.");

        //        const string sql = @"
        //    MERGE [dbo].[UserSettings_cookieName] AS target
        //    USING (VALUES (@Id, @CookieName, @CookieValue, @CookieDays)) AS source ([Id], [cookieName], [cookieValue], [cookieDays])
        //        ON target.[Id] = source.[Id]
        //    WHEN MATCHED THEN
        //        UPDATE SET
        //            cookieName  = COALESCE(source.[cookieName], target.[cookieName]),
        //            cookieValue = COALESCE(source.[cookieValue], target.[cookieValue]),
        //            cookieDays  = COALESCE(source.[cookieDays], target.[cookieDays]),
        //            updateDT    = GETDATE()
        //    WHEN NOT MATCHED THEN
        //        INSERT ([Id], [cookieName], [cookieValue], [cookieDays], [InsertDT], [updateDT])
        //        VALUES (source.[Id], source.[cookieName], source.[cookieValue], source.[cookieDays], GETDATE(), GETDATE());";

        //        var parameters = new List<SqlParameter>
        //{
        //    new SqlParameter("@Id", SqlDbType.NVarChar, 450) { Value = UserId },
        //    new SqlParameter("@CookieName", SqlDbType.NVarChar, 200) { Value = (object?)cookieName ?? DBNull.Value },
        //    new SqlParameter("@CookieValue", SqlDbType.NVarChar, 200) { Value = (object?)cookieValue ?? DBNull.Value },
        //    new SqlParameter("@CookieDays", SqlDbType.Int) { Value = (object?)cookieDays ?? DBNull.Value }
        //};

        //        DataAccessModule.executeNonQuery(sql, parameters);
        //    }

        //}
    }
}
