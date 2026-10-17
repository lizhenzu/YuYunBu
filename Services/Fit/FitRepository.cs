// Data/Fitness/FitRepository.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;              // ✅ 통일: Microsoft.Data.SqlClient
using Microsoft.Extensions.Configuration;    // ✅ .NET Core/5+ DI

using BulletinBoard.Models;
using BulletinBoard.Helpers;

namespace BulletinBoard.Services
{
    public class FitRepository : IFitRepository
    {
        private readonly string _connStr;

        // ✅ .NET Core/5+ 사용 시
        public FitRepository(IConfiguration configuration)
        {
            var cs = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(cs))
                throw new InvalidOperationException("Missing connection string: DefaultConnection");

            _connStr = cs;
        }

        // ❌ .NET Framework 전용 생성자는 제거(필요하면 별도 브랜치/파일로 운영)
        // using System.Configuration 도 제거했습니다.

        public async Task<PagedResult<ExerciseDocListItem>> GetExerciseDocsAsync(int page, int pageSize, string keyword)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 20;

            int offset = (page - 1) * pageSize;

            var sbWhere = new StringBuilder("WHERE 1=1 ");
            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                sbWhere.Append(@"
AND (
    d.TitleKo LIKE @kw OR d.TitleEn LIKE @kw OR d.Subtitle LIKE @kw
    OR d.Keywords LIKE @kw
    OR EXISTS (SELECT 1 FROM fit.ExerciseChips c WHERE c.ExerciseDocId = d.ExerciseDocId AND c.ChipText LIKE @kw)
)");
                parameters.Add(new SqlParameter("@kw", SqlDbType.NVarChar, 200)
                {
                    Value = "%" + keyword.Trim() + "%"
                });
            }

            string sql =
@"SELECT COUNT(*)
  FROM fit.ExerciseDoc d
  " + sbWhere + @";

SELECT
    d.ExerciseDocId, d.Slug, d.TitleKo, d.Subtitle, d.Audience, d.LevelText, d.Keywords, d.CreatedAt, d.UpdatedAt, isnull(d.Visited, 0) Visited,
    (SELECT TOP 1 Url FROM fit.Illustrations i WHERE i.ExerciseDocId = d.ExerciseDocId ORDER BY i.SortOrder ASC, i.IllustrationId ASC) AS CoverUrl
FROM fit.ExerciseDoc d
" + sbWhere + @"
ORDER BY d.ExerciseDocId 
OFFSET @offset ROWS FETCH NEXT @take ROWS ONLY;";

            parameters.Add(new SqlParameter("@offset", SqlDbType.Int) { Value = offset });
            parameters.Add(new SqlParameter("@take", SqlDbType.Int) { Value = pageSize });

            var result = new PagedResult<ExerciseDocListItem>
            {
                Page = page,
                PageSize = pageSize
            };

            using (var conn = new SqlConnection(_connStr))
            {
                await conn.OpenAsync();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddRange(parameters.ToArray());

                    await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // total
                        if (await reader.ReadAsync())
                        {
                            result.TotalCount = reader.GetInt32(0);
                        }

                        // items
                        if (await reader.NextResultAsync())
                        {
                            var list = new List<ExerciseDocListItem>();
                            while (await reader.ReadAsync())
                            {
                                var item = new ExerciseDocListItem
                                {
                                    ExerciseDocId = reader.GetInt32(reader.GetOrdinal("ExerciseDocId")),
                                    Visited = reader.GetInt32(reader.GetOrdinal("Visited")),
                                    Slug = reader["Slug"] as string ?? "",
                                    TitleKo = reader["TitleKo"] as string ?? "",
                                    Subtitle = reader["Subtitle"] as string ?? "",
                                    Audience = reader["Audience"] as string ?? "",
                                    LevelText = reader["LevelText"] as string ?? "",
                                    Keywords = reader["Keywords"] as string ?? "",
                                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                    UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                                    CoverUrl = reader["CoverUrl"] as string ?? ""
                                };

                                list.Add(item);
                            }

                            result.Items = list;
                        }
                    }
                }
            }

            return result;
        }

        public async Task IncrementVisitedAsync(int exerciseDocId)
        {
            const string sql = @"
UPDATE fit.ExerciseDoc
SET Visited = ISNULL(Visited, 0) + 1
WHERE ExerciseDocId = @id;
";

            using (var conn = new SqlConnection(_connStr))
            {
                await conn.OpenAsync();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = exerciseDocId });

                    // (선택) 로그 필요하면 주석 해제
                    // await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<ExerciseDocDetail?> GetExerciseDocDetailAsync(int exerciseDocId)
        {
            const string sql =
@"
SELECT ExerciseDocId, Slug, TitleKo, TitleEn, Subtitle, Audience, LevelText, Keywords, Description
FROM fit.ExerciseDoc
WHERE ExerciseDocId = @id;

SELECT HtmlTitle, MetaDescription
FROM fit.SeoMeta
WHERE ExerciseDocId = @id;

SELECT ChipId, ChipText, SortOrder
FROM fit.ExerciseChips
WHERE ExerciseDocId = @id
ORDER BY SortOrder, ChipId;

SELECT IllustrationId, Url, AltText, SortOrder
FROM fit.Illustrations
WHERE ExerciseDocId = @id
ORDER BY SortOrder, IllustrationId;

SELECT SectionId, SectionType, Eyebrow, Title, Subtitle, Body, SortOrder
FROM fit.Sections
WHERE ExerciseDocId = @id
ORDER BY SortOrder, SectionId;

SELECT ListItemId, SectionId, ItemType, Content, SortOrder
FROM fit.SectionListItems
WHERE SectionId IN (SELECT SectionId FROM fit.Sections WHERE ExerciseDocId = @id)
ORDER BY SectionId, SortOrder, ListItemId;

SELECT PrescriptionId, SectionId, Note
FROM fit.Prescriptions
WHERE SectionId IN (SELECT SectionId FROM fit.Sections WHERE ExerciseDocId = @id);

SELECT PrescriptionRowId, PrescriptionId, LabelText, ValueText, SortOrder
FROM fit.PrescriptionRows
WHERE PrescriptionId IN (SELECT p.PrescriptionId FROM fit.Prescriptions p
                         WHERE p.SectionId IN (SELECT SectionId FROM fit.Sections WHERE ExerciseDocId = @id))
ORDER BY PrescriptionId, SortOrder, PrescriptionRowId;

SELECT NavLinkId, Label, Href, StyleHint, SortOrder
FROM fit.NavigationLinks
WHERE ExerciseDocId = @id
ORDER BY SortOrder, NavLinkId;
";

            using (var conn = new SqlConnection(_connStr))
            {
                await conn.OpenAsync();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(new SqlParameter("@id", SqlDbType.Int) { Value = exerciseDocId });

                    await DbLogger.LogDebugAsync($"{SqlDebugHelper.ToDebugSql(cmd)}");

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        ExerciseDocDetail? detail = null;

                        // 1) doc
                        if (await reader.ReadAsync())
                        {
                            detail = new ExerciseDocDetail
                            {
                                ExerciseDocId = reader.GetInt32(reader.GetOrdinal("ExerciseDocId")),
                                Slug = reader["Slug"] as string ?? "",
                                TitleKo = reader["TitleKo"] as string ?? "",
                                TitleEn = reader["TitleEn"] as string ?? "",
                                Subtitle = reader["Subtitle"] as string ?? "",
                                Audience = reader["Audience"] as string ?? "",
                                LevelText = reader["LevelText"] as string ?? "",
                                Keywords = reader["Keywords"] as string ?? "",
                                Description = reader["Description"] as string ?? ""
                            };
                        }
                        else
                        {
                            return null; // not found
                        }

                        // 2) SEO
                        if (await reader.NextResultAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                detail.Seo = new SeoMetaDto
                                {
                                    HtmlTitle = reader["HtmlTitle"] as string ?? "",
                                    MetaDescription = reader["MetaDescription"] as string ?? ""
                                };
                            }
                        }

                        // 3) Chips
                        if (await reader.NextResultAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var c = new ChipDto
                                {
                                    ChipId = reader.GetInt32(reader.GetOrdinal("ChipId")),
                                    ChipText = reader["ChipText"] as string ?? "",
                                    SortOrder = reader.GetInt32(reader.GetOrdinal("SortOrder"))
                                };
                                detail.Chips.Add(c);
                            }
                        }

                        // 4) Illustrations
                        if (await reader.NextResultAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var il = new IllustrationDto
                                {
                                    IllustrationId = reader.GetInt32(reader.GetOrdinal("IllustrationId")),
                                    Url = reader["Url"] as string ?? "",
                                    AltText = reader["AltText"] as string ?? "",
                                    SortOrder = reader.GetInt32(reader.GetOrdinal("SortOrder"))
                                };
                                detail.Illustrations.Add(il);
                            }
                        }

                        // 5) Sections
                        var sectionMap = new Dictionary<int, SectionDto>();
                        if (await reader.NextResultAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var s = new SectionDto
                                {
                                    SectionId = reader.GetInt32(reader.GetOrdinal("SectionId")),
                                    SectionType = reader["SectionType"] as string ?? "",
                                    Eyebrow = reader["Eyebrow"] as string ?? "",
                                    Title = reader["Title"] as string ?? "",
                                    Subtitle = reader["Subtitle"] as string ?? "",
                                    Body = reader["Body"] as string ?? "",
                                    SortOrder = reader.GetInt32(reader.GetOrdinal("SortOrder"))
                                };

                                sectionMap[s.SectionId] = s;
                                detail.Sections.Add(s);
                            }
                        }

                        // 6) Section list items
                        if (await reader.NextResultAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int sid = reader.GetInt32(reader.GetOrdinal("SectionId"));
                                if (sectionMap.TryGetValue(sid, out var s))
                                {
                                    var it = new SectionListItemDto
                                    {
                                        ListItemId = reader.GetInt32(reader.GetOrdinal("ListItemId")),
                                        ItemType = reader["ItemType"] as string ?? "",
                                        Content = reader["Content"] as string ?? "",
                                        SortOrder = reader.GetInt32(reader.GetOrdinal("SortOrder"))
                                    };
                                    s.Items.Add(it);
                                }
                            }
                        }

                        // 7) Prescriptions
                        var presMap = new Dictionary<int, PrescriptionDto>();
                        if (await reader.NextResultAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int sid = reader.GetInt32(reader.GetOrdinal("SectionId"));

                                var p = new PrescriptionDto
                                {
                                    PrescriptionId = reader.GetInt32(reader.GetOrdinal("PrescriptionId")),
                                    Note = reader["Note"] as string ?? ""
                                };
                                presMap[p.PrescriptionId] = p;

                                if (sectionMap.TryGetValue(sid, out var s))
                                {
                                    s.Prescription = p;
                                }
                            }
                        }

                        // 8) Prescription rows
                        if (await reader.NextResultAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int pid = reader.GetInt32(reader.GetOrdinal("PrescriptionId"));
                                if (presMap.TryGetValue(pid, out var p))
                                {
                                    var r = new PrescriptionRowDto
                                    {
                                        PrescriptionRowId = reader.GetInt32(reader.GetOrdinal("PrescriptionRowId")),
                                        LabelText = reader["LabelText"] as string ?? "",
                                        ValueText = reader["ValueText"] as string ?? "",
                                        SortOrder = reader.GetInt32(reader.GetOrdinal("SortOrder"))
                                    };
                                    p.Rows.Add(r);
                                }
                            }
                        }

                        // 9) Navigation
                        if (await reader.NextResultAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var nav = new NavigationLinkDto
                                {
                                    NavLinkId = reader.GetInt32(reader.GetOrdinal("NavLinkId")),
                                    Label = reader["Label"] as string ?? "",
                                    Href = reader["Href"] as string ?? "",
                                    StyleHint = reader["StyleHint"] as string ?? "",
                                    SortOrder = reader.GetInt32(reader.GetOrdinal("SortOrder"))
                                };
                                detail.Navigation.Add(nav);
                            }
                        }

                        return detail;
                    }
                }
            }
        }
    }
}