using BulletinBoard.Common;
using Microsoft.Extensions.Logging;
using BulletinBoard.Common;
using System;
using System.Data; 
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using BulletinBoard.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlBessingACouple
    {
        
        // ── 검증 칸 (IsVerified · VerifiedDT · VerifiedBy) ──────────────────
        // SQL\AddVerifyColumns_BessingCouple_20261011.sql 을 실행하기 전에도 화면이 깨지지 않도록
        // 칸이 있는지 먼저 확인한다. (결과는 60초 동안 기억 — SQL 을 실행하면 곧 켜진다)
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, (bool Ready, long At)> _verifyReady
            = new System.Collections.Concurrent.ConcurrentDictionary<string, (bool, long)>();

        public bool HasVerifyColumns(string Kind)
        {
            long now = Environment.TickCount64;
            if (_verifyReady.TryGetValue(Kind, out var c) && now - c.At < 60_000) return c.Ready;

            bool ready = false;
            try
            {
                // ⚠️ COL_LENGTH 는 smallint 를 돌려준다 — executeScalar 가 (Int32) 로 바꾸다 실패해 0 이 되므로 int 로 바꿔서 받는다
                ready = DataAccessModule.executeScalar($" select cast(isnull(COL_LENGTH('dbo.Bessing{Kind}', 'IsVerified'), 0) as int) ") > 0;
            }
            catch { ready = false; }

            _verifyReady[Kind] = (ready, now);
            return ready;
        }

        /// <summary>검증 완료로 표시 — 이미 검증된 기도문은 그대로 둔다. 칸이 없으면 false.</summary>
        public bool VerifyBessingACouple(string Kind, int Id, string by)
        {
            if (!HasVerifyColumns(Kind)) return false;

            string who = (by ?? string.Empty).Replace("'", "''");
            if (who.Length > 100) who = who.Substring(0, 100);

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat($" update [dbo].[Bessing{Kind}] "); sb.Append(Environment.NewLine);
            sb.AppendFormat($" set IsVerified = 1, VerifiedDT = DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()), VerifiedBy = N'{who}' "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] = {0} and isnull(IsVerified, 0) = 0 ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
            return true;
        }

        /// <summary>검증 취소 — 다시 "검증 전" 으로 되돌린다. 칸이 없으면 false.</summary>
        public bool UnverifyBessingACouple(string Kind, int Id)
        {
            if (!HasVerifyColumns(Kind)) return false;

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat($" update [dbo].[Bessing{Kind}] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set IsVerified = 0, VerifiedDT = null, VerifiedBy = null "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] = {0} and isnull(IsVerified, 0) = 1 ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
            return true;
        }

        public DataSet fetchBessingACouple(string Kind, int Id, string ExposeDate)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" select top 100 ROW_NUMBER() OVER(ORDER BY  [UpdateDT]  )  sequenceNo ,");
            sb.AppendFormat("  Id, ");
            sb.AppendFormat("  PageNo, ");
            sb.AppendFormat("  isnull(PrayTitle,'') PrayTitle , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(PrayText,'') PrayText , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(PrayHolyWord,'') PrayHolyWord , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  UpdateDT , "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" [visited] , "); sb.Append(Environment.NewLine);
            if (HasVerifyColumns(Kind))
            {
                sb.AppendFormat("  cast(isnull(IsVerified, 0) as bit) IsVerified , VerifiedDT , VerifiedBy "); sb.Append(Environment.NewLine);
            }
            else
            {
                // 칸이 아직 없다 — 모델 속성과 이름·형식을 맞춘 빈 값
                sb.AppendFormat("  cast(0 as bit) IsVerified , cast(null as datetime) VerifiedDT , cast(null as nvarchar(100)) VerifiedBy "); sb.Append(Environment.NewLine);
            }

            sb.AppendFormat($" from [dbo].[Bessing{Kind}] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where 1 = 1 "); sb.Append(Environment.NewLine);

            if (Id > 0)
            {
                sb.AppendFormat(" and Id = {0} ", Id); sb.Append(Environment.NewLine);
            }

            // 2026-10-04: 정렬이 없으면 DB 상태에 따라 순서가 바뀌어 "오늘의 기도"(날짜 순환)가 흔들린다 → Id 순으로 고정
            sb.AppendFormat(" order by Id "); sb.Append(Environment.NewLine);

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }



        // 2026-10-15: 글은 매개변수로 넘긴다 — 기도문에 작은따옴표(')가 들어가도 저장이 깨지지 않고, SQL 주입도 막는다.
        //             (Kind 는 컨트롤러에서 Wife · Husband 만 받으므로 표 이름에는 그대로 쓴다)
        private static System.Data.SqlClient.SqlParameter NText(string name, string value)
            => new System.Data.SqlClient.SqlParameter(name, SqlDbType.NVarChar, -1) { Value = (object)SharedComm.TrimString(value) ?? string.Empty };

        /// <summary>새 기도문. PageNo 가 0 이하면 맨 끝 번호(+1)로 넣는다.</summary>
        public void CreateBessingACouple(string Kind, int PageNo, string PrayTitle, string PrayText, string PrayHolyWord)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat($" insert into [dbo].[Bessing{Kind}] ([PageNo], [PrayTitle], [PrayText], [PrayHolyWord], [InsertDT], [updateDT]) "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" values ( ");
            sb.AppendFormat($" case when @PageNo > 0 then @PageNo else (select isnull(max(PageNo), 0) + 1 from [dbo].[Bessing{Kind}]) end, ");
            sb.AppendFormat(" @PrayTitle, @PrayText, @PrayHolyWord, ");
            sb.AppendFormat($"  DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()),  ");
            sb.AppendFormat($"  DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE())  ");
            sb.AppendFormat(" ) "); sb.Append(Environment.NewLine);

            DataAccessModule.executeNonQuery(sb.ToString(), new System.Collections.Generic.List<System.Data.SqlClient.SqlParameter>
            {
                new System.Data.SqlClient.SqlParameter("@PageNo", SqlDbType.Int) { Value = PageNo },
                NText("@PrayTitle", PrayTitle),
                NText("@PrayText", PrayText),
                NText("@PrayHolyWord", PrayHolyWord),
            });
        }

        /// <summary>가장 최근에 넣은 기도문 Id (새로 쓴 기도문으로 바로 가기 위해)</summary>
        public int GetMaxId(string Kind)
            => DataAccessModule.executeScalar($" select cast(isnull(max(Id), 0) as int) from [dbo].[Bessing{Kind}] ");

        /// <summary>다음에 들어갈 번호 (화면 안내용)</summary>
        public int GetNextPageNo(string Kind)
            => DataAccessModule.executeScalar($" select cast(isnull(max(PageNo), 0) + 1 as int) from [dbo].[Bessing{Kind}] ");

        public void UpdateBessingACouple(string Kind, int Id, int PageNo, string PrayTitle, string PrayText, string PrayHolyWord)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat($" update [dbo].[Bessing{Kind}] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set PageNo       = @PageNo , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("     PrayTitle    = @PrayTitle , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("     PrayText     = @PrayText , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("     PrayHolyWord = @PrayHolyWord , "); sb.Append(Environment.NewLine);
            sb.AppendFormat($"    [updateDT]   = DATEADD(HOUR,{ConstatsCommon.timeDifference16}, GETDATE()) "); sb.Append(Environment.NewLine);
            // 내용을 고쳤으면 다시 검증해야 하므로 검증 표시를 푼다 (2026-10-11)
            if (HasVerifyColumns(Kind))
            {
                sb.AppendFormat("    , IsVerified = 0, VerifiedDT = null, VerifiedBy = null "); sb.Append(Environment.NewLine);
            }
            sb.AppendFormat(" where [Id]       = @Id "); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString(), new System.Collections.Generic.List<System.Data.SqlClient.SqlParameter>
            {
                new System.Data.SqlClient.SqlParameter("@Id", SqlDbType.Int) { Value = Id },
                new System.Data.SqlClient.SqlParameter("@PageNo", SqlDbType.Int) { Value = PageNo },
                NText("@PrayTitle", PrayTitle),
                NText("@PrayText", PrayText),
                NText("@PrayHolyWord", PrayHolyWord),
            });
        }

        public void HitBessingACouple(string Kind, int Id)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat($" update [dbo].[Bessing{Kind}] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set visited = visited + 1 ", Id); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public void DecreasetHitACouple(string Kind, int Id)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat($" update [dbo].[Bessing{Kind}] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set visited = visited - 1 ", Id); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
        public void DeleteBessingACouple(string Kind, int Id)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat($" delete from [dbo].[Bessing{Kind}] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
    }
}