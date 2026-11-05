using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace BulletinBoard.Common
{
    /// <summary>묶음 실행 중 오류. 몇 번째 묶음인지 함께 알려 준다 (전체는 되돌린 뒤 던진다)</summary>
    public class SqlBatchException : Exception
    {
        public int BatchNo { get; }
        public int BatchCount { get; }

        public SqlBatchException(int batchNo, int batchCount, Exception inner)
            : base(inner.Message, inner)
        {
            BatchNo = batchNo;
            BatchCount = batchCount;
        }
    }

    /// <summary>
    /// SSMS 처럼 GO 로 나눈 SQL 을 실행한다 (2026-11-05).
    /// GO 는 SQL 문이 아니라 SSMS · sqlcmd 의 구분자라서 SqlCommand 에 그대로 넘기면 문법 오류가 난다.
    /// </summary>
    public static class SqlBatchRunner
    {
        private static readonly Regex GoLine = new Regex(@"\G[ \t]*GO[ \t]*(--[^\r\n]*)?(\r?\n|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>줄에 GO 만 있으면 그곳에서 나눈다. 문자열 · 주석 · 대괄호 안의 GO 는 나누지 않는다</summary>
        public static List<string> Split(string? sql)
        {
            var batches = new List<string>();
            if (string.IsNullOrWhiteSpace(sql)) return batches;

            var current = new StringBuilder();
            void Flush()
            {
                if (!string.IsNullOrWhiteSpace(current.ToString())) batches.Add(current.ToString());
                current.Clear();
            }

            int i = 0;
            bool lineStart = true;
            while (i < sql.Length)
            {
                if (lineStart)
                {
                    var go = GoLine.Match(sql, i);
                    if (go.Success)
                    {
                        Flush();
                        i += go.Length;
                        continue;   // 다음 칸도 줄의 시작이다
                    }
                    lineStart = false;
                }

                char c = sql[i];
                char next = i + 1 < sql.Length ? sql[i + 1] : '\0';
                int end;

                if (c == '\'' || c == '"')      end = SkipQuoted(sql, i, c);
                else if (c == '[')              end = SkipQuoted(sql, i, ']');
                else if (c == '-' && next == '-') end = SkipLineComment(sql, i);
                else if (c == '/' && next == '*') end = SkipBlockComment(sql, i);
                else
                {
                    current.Append(c);
                    i++;
                    lineStart = c == '\n';
                    continue;
                }

                current.Append(sql, i, end - i);
                i = end;
            }
            Flush();
            return batches;
        }

        /// <summary>
        /// 한 연결 · 한 트랜잭션으로 차례로 실행하고 바뀐 행 수를 돌려준다.
        /// 중간에 오류가 나면 앞 묶음까지 모두 되돌린다
        /// </summary>
        public static int Run(string connectionString, IReadOnlyList<string> batches)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            int affected = 0;
            for (int n = 0; n < batches.Count; n++)
            {
                try
                {
                    using var command = new SqlCommand(batches[n], connection, transaction);
                    affected += Math.Max(0, command.ExecuteNonQuery());
                }
                catch (Exception ex) when (ex is SqlException || ex is InvalidOperationException)
                {
                    throw new SqlBatchException(n + 1, batches.Count, ex);
                }
            }

            try
            {
                transaction.Commit();
            }
            catch (Exception ex) when (ex is SqlException || ex is InvalidOperationException)
            {
                // 붙여 넣은 SQL 안의 ROLLBACK 등으로 트랜잭션이 이미 끝난 경우
                throw new SqlBatchException(batches.Count, batches.Count, ex);
            }
            return affected;
        }

        // 여는 문자부터 닫는 문자 다음 위치까지. 겹쳐 쓴 닫는 문자('' · ]])는 글자로 본다
        private static int SkipQuoted(string sql, int start, char close)
        {
            int i = start + 1;
            while (i < sql.Length)
            {
                if (sql[i] == close)
                {
                    if (i + 1 < sql.Length && sql[i + 1] == close) { i += 2; continue; }
                    return i + 1;
                }
                i++;
            }
            return sql.Length;
        }

        // 줄바꿈은 남겨 둔다 — 다음 줄의 GO 를 알아보기 위해서
        private static int SkipLineComment(string sql, int start)
        {
            int nl = sql.IndexOf('\n', start);
            return nl < 0 ? sql.Length : nl;
        }

        // T-SQL 의 블록 주석은 겹칠 수 있다
        private static int SkipBlockComment(string sql, int start)
        {
            int depth = 1, i = start + 2;
            while (i < sql.Length && depth > 0)
            {
                if (i + 1 < sql.Length && sql[i] == '/' && sql[i + 1] == '*') { depth++; i += 2; }
                else if (i + 1 < sql.Length && sql[i] == '*' && sql[i + 1] == '/') { depth--; i += 2; }
                else i++;
            }
            return i;
        }
    }
}
