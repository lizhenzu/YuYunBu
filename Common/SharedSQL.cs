using BulletinBoard.BusinessLayer;
using BulletinBoard.Enums;
using BulletinBoard.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using SkiaSharp;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using BulletinBoard.Helpers;

namespace BulletinBoard.Common
{
    public static class SharedSQL
    {
        public static string MakeSQL_ReadBible(BilbeProcessOption bilbeProcessOption)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat($"   select top 100 ROW_NUMBER() OVER(ORDER BY  {bilbeProcessOption.ToString()} desc) sequenceNo, MA.TotalProgressRate, MA.ReadDate,  "); sb.Append(Environment.NewLine);

            sb.AppendFormat("        isnull(UserId   , '') UserId,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("        isnull(LastName , '') LastName,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("        isnull(FirstName, '') FirstName,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("        isnull(MB.UserName , '') UserName,  "); sb.Append(Environment.NewLine);
            // 완독 횟수는 상관 서브쿼리로 읽는다.
            // 예전처럼 UserBibleProgressCompleted 를 join 하면 완독 기록이 여러 줄인 사람이
            // 그 수만큼 중복 행으로 불어나, 아래 TOP 100 자리를 잡아먹어 다른 사람이 잘려 나갔다.
            sb.AppendFormat("        isnull((SELECT MAX([readBibleAllCount]) FROM [dbo].[UserBibleProgressCompleted] WHERE UserName = MB.UserName), 0)  [readBibleAllCount],  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("        isnull((SELECT TOP 1 RoleCode FROM [dbo].[view_UserCompositeInfo_2026] WHERE UserName = MB.UserName), '') RoleCode,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("        isnull((SELECT TOP 1 RoleNM   FROM [dbo].[view_UserCompositeInfo_2026] WHERE UserName = MB.UserName), '') RoleNM,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("        isnull((SELECT TOP 1 ClassYear FROM [dbo].[view_UserCompositeInfo_2026] WHERE UserName = MB.UserName), 0) ClassYear,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("        isnull((SELECT TOP 1 ClassBan  FROM [dbo].[view_UserCompositeInfo_2026] WHERE UserName = MB.UserName), 0) ClassBan,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("        ''                   ProfilePicture "); sb.Append(Environment.NewLine);

            sb.AppendFormat("      FROM (");
            sb.AppendFormat("             SELECT UserId,");
            sb.AppendFormat("             COUNT(*) * 100.0 / (SELECT COUNT(*) FROM eBible) AS TotalProgressRate , ");
            sb.AppendFormat("             MAX(ReadDate) AS ReadDate");
            sb.AppendFormat("             FROM UserBibleProgress");
            sb.AppendFormat("             GROUP BY UserId");
            sb.AppendFormat("           ) MA");
            sb.AppendFormat("    join [Identity].[User]                  MB on MA.UserId = MB.Id "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    where 1 = 1   "); sb.Append(Environment.NewLine);
            // TOP 100 에 바깥 ORDER BY 가 없으면 어느 100명이 올지 정해지지 않는다.
            // (정렬은 ROW_NUMBER() 안에만 있어서 결과 순서를 보장하지 못한다.)
            // 진도율이 높은 사람부터 뽑아야 100% 완독자가 잘리지 않는다.
            sb.AppendFormat($"    order by {bilbeProcessOption.ToString()} desc   "); sb.Append(Environment.NewLine);
            DbLogger.LogDebugAsync(sb.ToString());

            return sb.ToString();
        }

        public static string MakeSQL_ReadBibleCompleted()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat($"   select top 100 ROW_NUMBER() OVER(ORDER BY  (LastName + FirstName)  desc) sequenceNo, "); sb.Append(Environment.NewLine);

            sb.AppendFormat("    isnull(Id          , '') UserId,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    isnull(LastName    , '') LastName,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    isnull(FirstName   , '') FirstName,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    isnull(MB.UserName , '') UserName,  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    ''                      ProfilePicture, "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    readBibleAllCount  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    FROM UserBibleProgressCompleted MA");
            sb.AppendFormat("    join [Identity].[User] MB on MA.UserName = MB.UserName "); sb.Append(Environment.NewLine);
            sb.AppendFormat("    where 1 = 1   "); sb.Append(Environment.NewLine);
            DbLogger.LogDebugAsync(sb.ToString());

            return sb.ToString();
        }
    }
}
