using BulletinBoard.Helpers;
using BulletinBoard.Models;
using BulletinBoard.Services.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System.Data;
using BulletinBoard.Common;

namespace BulletinBoard.Services
{
    public class AwardItemService : IAwardItemService
    {
        private readonly string _connStr;

        public AwardItemService(IConfiguration config)
        {
            _connStr = config.GetConnectionString("DefaultConnection")
                      ?? throw new InvalidOperationException("DefaultConnection is missing.");
        }

        private SqlConnection CreateConn() => new SqlConnection(_connStr);
        private static string Norm(string? s) => (s ?? "").Trim();

        public async Task<List<AwardItem>> GetAllAsync(ProductDisplayOpton option, bool includeHidden)
        {
            var sb = new System.Text.StringBuilder(@"
                                                    SELECT ProductID, ProductName, ProductDescription, UnitPrice, CreatedDate, ModifiedDate, SourceType, IsHidden, ExternalUrl
                                                    FROM dbo.AwardItem WITH (NOLOCK)
                                                    WHERE 1 = 1
                                                    ");

            // ✅ 숨김 제외 조건
            if (!includeHidden)
            {
                sb.AppendLine("AND ISNULL(IsHidden,0)=0");
            }

            // ✅ 가격 규칙은 GiftPriceRule 한 곳에서만 만든다 (managePrice 처럼 조건이 있는 목록용)
            if (BulletinBoard.Services.GiftCatalog.GiftPriceRule.IsDescriptionMissingOnly(option))
            {
                sb.AppendLine(BulletinBoard.Services.GiftCatalog.GiftPriceRule.WhereSql(option));
            }

            sb.AppendLine($" order by ISNULL([ModifiedDate],[CreatedDate]) asc");

            //sb.AppendLine("ORDER BY ProductID DESC;");

            var list = new List<AwardItem>();
            using var conn = CreateConn();
            using var cmd = new SqlCommand(sb.ToString(), conn);
            await conn.OpenAsync();
 
            DbLogger.LogDebugAsync($" Birth AwardItem : {SqlDebugHelper.ToDebugSql(cmd)}");


            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new AwardItem
                {
                    ProductID = r.GetInt32(0),
                    ProductName = r.GetString(1),
                    ProductDescription = r.IsDBNull(2) ? null : r.GetString(2),
                    UnitPrice = r.GetInt32(3),
                    CreatedDate = r.IsDBNull(4) ? null : r.GetDateTime(4),
                    ModifiedDate = r.IsDBNull(5) ? null : r.GetDateTime(5),
                    SourceType = r.IsDBNull(6) ? null : r.GetString(6),
                    IsHidden = !r.IsDBNull(7) && r.GetBoolean(7),
                    ExternalUrl = r.IsDBNull(8) ? null : r.GetString(8),
                });
            }
            return list;
        }

        public async Task<AwardItem?> GetByIdAsync(int productId, bool includeHidden)
        {
            var sql = @"
SELECT TOP 1 ProductID, ProductName, ProductDescription, UnitPrice, CreatedDate, ModifiedDate, SourceType, IsHidden, ExternalUrl
FROM dbo.AwardItem WITH (NOLOCK)
WHERE ProductID=@ProductID
" + (includeHidden ? "" : "AND ISNULL(IsHidden,0)=0") + ";";

            using var conn = CreateConn();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@ProductID", SqlDbType.Int) { Value = productId });

            await conn.OpenAsync();
            using var r = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            if (!await r.ReadAsync()) return null;

            return new AwardItem
            {
                ProductID = r.GetInt32(0),
                ProductName = r.GetString(1),
                ProductDescription = r.IsDBNull(2) ? null : r.GetString(2),
                UnitPrice = r.GetInt32(3),
                CreatedDate = r.IsDBNull(4) ? null : r.GetDateTime(4),
                ModifiedDate = r.IsDBNull(5) ? null : r.GetDateTime(5),
                SourceType = r.IsDBNull(6) ? null : r.GetString(6),
                IsHidden = !r.IsDBNull(7) && r.GetBoolean(7),
                ExternalUrl = r.IsDBNull(8) ? null : r.GetString(8),
            };
        }

        public async Task<int> CreateAsync(AwardItem item)
        {
            var name = Norm(item.ProductName);
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("ProductName is required.");

            var sourceType = Norm(item.SourceType);
            if (string.IsNullOrWhiteSpace(sourceType)) sourceType = "AdminUpload";

            const string sql = @"
INSERT INTO dbo.AwardItem (ProductName, ProductDescription, UnitPrice, SourceType, ModifiedDate, IsHidden, ExternalUrl)
VALUES (@ProductName, @ProductDescription, @UnitPrice, @SourceType, GETDATE(), ISNULL(@IsHidden,0), @ExternalUrl);
SELECT CAST(SCOPE_IDENTITY() as int);";

            using var conn = CreateConn();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add(new SqlParameter("@ProductName", SqlDbType.NVarChar, 100) { Value = name });
            cmd.Parameters.Add(new SqlParameter("@ProductDescription", SqlDbType.NVarChar, 500)
            {
                Value = (object?)item.ProductDescription ?? DBNull.Value
            });
            cmd.Parameters.Add(new SqlParameter("@UnitPrice", SqlDbType.Int) { Value = item.UnitPrice });
            cmd.Parameters.Add(new SqlParameter("@SourceType", SqlDbType.NVarChar, 100) { Value = sourceType });
            cmd.Parameters.Add(new SqlParameter("@IsHidden", SqlDbType.Bit) { Value = item.IsHidden });
            cmd.Parameters.Add(new SqlParameter("@ExternalUrl", SqlDbType.NVarChar, 1000)
            {
                Value = (object?)item.ExternalUrl ?? DBNull.Value
            });

            await conn.OpenAsync();
            var idObj = await cmd.ExecuteScalarAsync();
            return (idObj == null || idObj == DBNull.Value) ? 0 : Convert.ToInt32(idObj);
        }

        public async Task UpdateAsync(AwardItem item)
        {
            var name = Norm(item.ProductName);
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("ProductName is required.");

            var sourceType = Norm(item.SourceType);
            if (string.IsNullOrWhiteSpace(sourceType)) sourceType = "AdminUpload";

            const string sql = @"
UPDATE dbo.AwardItem
SET ProductName=@ProductName,
    ProductDescription=@ProductDescription,
    UnitPrice=@UnitPrice,
    SourceType=@SourceType,
    ModifiedDate=GETDATE(),
    IsHidden=ISNULL(@IsHidden,0),
    ExternalUrl=@ExternalUrl
WHERE ProductID=@ProductID;";

            using var conn = CreateConn();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add(new SqlParameter("@ProductID", SqlDbType.Int) { Value = item.ProductID });
            cmd.Parameters.Add(new SqlParameter("@ProductName", SqlDbType.NVarChar, 100) { Value = name });
            cmd.Parameters.Add(new SqlParameter("@ProductDescription", SqlDbType.NVarChar, 500)
            {
                Value = (object?)item.ProductDescription ?? DBNull.Value
            });
            cmd.Parameters.Add(new SqlParameter("@UnitPrice", SqlDbType.Int) { Value = item.UnitPrice });
            cmd.Parameters.Add(new SqlParameter("@SourceType", SqlDbType.NVarChar, 100) { Value = sourceType });
            cmd.Parameters.Add(new SqlParameter("@IsHidden", SqlDbType.Bit) { Value = item.IsHidden });
            cmd.Parameters.Add(new SqlParameter("@ExternalUrl", SqlDbType.NVarChar, 1000)
            {
                Value = (object?)item.ExternalUrl ?? DBNull.Value
            });

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        // ✅ 값 지정(기존 방식)
        public async Task SetHiddenAsync(int productId, bool isHidden)
        {
            const string sql = @"
UPDATE dbo.AwardItem
SET IsHidden=@IsHidden,
    ModifiedDate=GETDATE()
WHERE ProductID=@ProductID;";

            using var conn = CreateConn();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add(new SqlParameter("@ProductID", SqlDbType.Int) { Value = productId });
            cmd.Parameters.Add(new SqlParameter("@IsHidden", SqlDbType.Bit) { Value = isHidden });

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        // ✅ 서버에서 직접 토글 (중복 제거하고 이 1개만 남기기)
        public async Task ToggleHiddenAsync(int productId)
        {
            const string sql = @"
UPDATE dbo.AwardItem
SET IsHidden = CASE WHEN ISNULL(IsHidden,0)=1 THEN 0 ELSE 1 END,
    ModifiedDate = GETDATE()
WHERE ProductID=@ProductID;";

            using var conn = CreateConn();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add(new SqlParameter("@ProductID", SqlDbType.Int) { Value = productId });

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int productId)
        {
            const string sql = @"DELETE FROM dbo.AwardItem WHERE ProductID=@ProductID;";
            using var conn = CreateConn();
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add(new SqlParameter("@ProductID", SqlDbType.Int) { Value = productId });

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task SaveImageJpgAsync(int productId, IFormFile imageFile, string webRootPath)
        {
            if (imageFile == null || imageFile.Length <= 0)
                throw new InvalidOperationException("Image is empty.");

            var ct = (imageFile.ContentType ?? "").ToLowerInvariant();
            if (!(ct.Contains("jpeg") || ct.Contains("jpg")))
                throw new InvalidOperationException("Only JPG(JPEG) image is allowed. (crop 결과를 JPG로 업로드해 주세요)");

            var dir = Path.Combine(webRootPath, "images", "AwardItem");
            Directory.CreateDirectory(dir);

            var filePath = Path.Combine(dir, $"{productId}.jpg");
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await imageFile.CopyToAsync(fs);
        }

        public Task DeleteImageAsync(int productId, string webRootPath)
        {
            var dir = Path.Combine(webRootPath, "images", "AwardItem");
            var filePath = Path.Combine(dir, $"{productId}.jpg");

            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch { }

            return Task.CompletedTask;
        }
    }
}
