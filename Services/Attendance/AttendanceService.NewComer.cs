using BulletinBoard.Helpers;
using BulletinBoard.Services.Abstract;
using BulletinBoard.ViewModels.Attendance;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BulletinBoard.Services
{
    public partial class AttendanceService : IAttendanceService
    {
        public async Task<NewComerIndexVM> GetNewComerIndexAsync(string? keyword)
        {
            var vm = new NewComerIndexVM
            {
                Keyword = keyword?.Trim() ?? string.Empty
            };

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            vm.ActiveList = await GetNewComerListInternalAsync(conn, vm.Keyword, "ACTIVE");
            vm.CompletedList = await GetNewComerListInternalAsync(conn, vm.Keyword, "COMPLETED");

            return vm;
        }

        public async Task<NewComerDetailVM?> GetNewComerDetailAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var info = await GetNewComerInfoInternalAsync(conn, id);
            if (info == null)
            {
                return null;
            }

            var visits = await GetNewComerVisitsInternalAsync(conn, id);

            return new NewComerDetailVM
            {
                Info = info,
                Visits = visits
            };
        }

        public async Task<int> SaveNewComerAsync(NewComerSaveModel model, string? userName)
        {
            if (model == null
                || string.IsNullOrWhiteSpace(model.ChildName)
                || string.IsNullOrWhiteSpace(model.ParentName)
                || string.IsNullOrWhiteSpace(model.Phone)
                || model.FirstVisitDate == null
                || !model.BirthMonth.HasValue
                || !model.BirthDay.HasValue
                || !model.ClassYear.HasValue
                || !model.ClassBan.HasValue)
            {
                return 0;
            }

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var tx = conn.BeginTransaction();

            try
            {
                int personId;
                int newComerId;

                if (model.Id > 0)
                {
                    personId = await GetPersonIdByNewComerIdAsync(conn, tx, model.Id);
                    if (personId <= 0)
                    {
                        tx.Rollback();
                        return 0;
                    }

                    await UpdatePersonMasterAsync(conn, tx, personId, model);
                    await ReplaceClassInfoAsync(conn, tx, personId, model.ClassYear.Value, model.ClassBan.Value);
                    newComerId = await UpdateNewComerInternalAsync(conn, tx, model, personId);

                    await EnsureFirstVisitExistsAsync(conn, tx, newComerId, model.FirstVisitDate.Value.Date);
                    await EnsureAttendanceCheckExistsAsync(conn, tx, personId, model.FirstVisitDate.Value.Date);
                }
                else
                {
                    personId = await InsertPersonMasterAsync(conn, tx, model);
                    await InsertClassInfoAsync(conn, tx, personId, model.ClassYear.Value, model.ClassBan.Value);
                    newComerId = await InsertNewComerInternalAsync(conn, tx, model, personId);

                    await EnsureFirstVisitExistsAsync(conn, tx, newComerId, model.FirstVisitDate.Value.Date);
                    await EnsureAttendanceCheckExistsAsync(conn, tx, personId, model.FirstVisitDate.Value.Date);
                }

                tx.Commit();
                return newComerId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteNewComerAsync(int id, string? userName)
        {
            if (id <= 0)
            {
                return false;
            }

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var tx = conn.BeginTransaction();

            try
            {
                var personId = await GetPersonIdByNewComerIdAsync(conn, tx, id);
                if (personId <= 0)
                {
                    tx.Rollback();
                    return false;
                }

                const string sql = @"
DELETE FROM dbo.AttendanceCheck_2026
WHERE PersonId = @PersonId;

DELETE FROM dbo.AttendanceNewComerVisit
WHERE NewComerId = @NewComerId;

DELETE FROM dbo.ClassInfo_2026
WHERE PersonID = @PersonId;

DELETE FROM dbo.AttendanceNewComer
WHERE Id = @NewComerId;

DELETE FROM dbo.PersonMaster_2026
WHERE PersonId = @PersonId;
";

                using var cmd = new SqlCommand(sql, conn, tx);
                cmd.Parameters.Add("@NewComerId", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@PersonId", SqlDbType.Int).Value = personId;

                await cmd.ExecuteNonQueryAsync();

                tx.Commit();
                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<bool> AddNewComerVisitTodayAsync(int newComerId, string? userName)
        {
            return await AddNewComerVisitByDateAsync(newComerId, DateHelper.GetKoreaNow().Date, userName);
        }

        public async Task<bool> AddNewComerVisitByDateAsync(int newComerId, DateTime visitDate, string? userName)
        {
            if (newComerId <= 0)
            {
                return false;
            }

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var tx = conn.BeginTransaction();

            try
            {
                var personId = await GetPersonIdByNewComerIdAsync(conn, tx, newComerId);
                if (personId <= 0)
                {
                    tx.Rollback();
                    return false;
                }

                const string visitSql = @"
IF NOT EXISTS
(
    SELECT 1
    FROM dbo.AttendanceNewComerVisit
    WHERE NewComerId = @NewComerId
      AND VisitDate = @VisitDate
)
BEGIN
    INSERT INTO dbo.AttendanceNewComerVisit
    (
        NewComerId,
        VisitDate,
        InsertDT,
        UpdateDT
    )
    VALUES
    (
        @NewComerId,
        @VisitDate,
        @InsertDT,
        @UpdateDT
    );

    SELECT 1;
END
ELSE
BEGIN
    SELECT 0;
END
";

                using (var cmd = new SqlCommand(visitSql, conn, tx))
                {
                    cmd.Parameters.Add("@NewComerId", SqlDbType.Int).Value = newComerId;
                    cmd.Parameters.Add("@VisitDate", SqlDbType.Date).Value = visitDate.Date;
                    cmd.Parameters.Add("@InsertDT", SqlDbType.DateTime).Value = DateHelper.GetKoreaNow();
                    cmd.Parameters.Add("@UpdateDT", SqlDbType.DateTime).Value = DateHelper.GetKoreaNow();

                    var inserted = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    if (inserted != 1)
                    {
                        tx.Rollback();
                        return false;
                    }
                }

                await EnsureAttendanceCheckExistsAsync(conn, tx, personId, visitDate.Date);

                tx.Commit();
                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteNewComerVisitAsync(int visitId, string? userName)
        {
            if (visitId <= 0)
            {
                return false;
            }

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var tx = conn.BeginTransaction();

            try
            {
                const string infoSql = @"
SELECT
    v.NewComerId,
    v.VisitDate,
    nc.PersonId
FROM dbo.AttendanceNewComerVisit v
INNER JOIN dbo.AttendanceNewComer nc
    ON nc.Id = v.NewComerId
WHERE v.Id = @VisitId;
";

                int personId = 0;
                DateTime visitDate = DateTime.MinValue;

                using (var cmd = new SqlCommand(infoSql, conn, tx))
                {
                    cmd.Parameters.Add("@VisitId", SqlDbType.Int).Value = visitId;

                    using var rdr = await cmd.ExecuteReaderAsync();
                    if (!await rdr.ReadAsync())
                    {
                        tx.Rollback();
                        return false;
                    }

                    personId = rdr["PersonId"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["PersonId"]);
                    visitDate = rdr["VisitDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(rdr["VisitDate"]).Date;
                }

                const string deleteVisitSql = @"
DELETE FROM dbo.AttendanceNewComerVisit
WHERE Id = @VisitId;

SELECT @@ROWCOUNT;
";

                using (var cmd = new SqlCommand(deleteVisitSql, conn, tx))
                {
                    cmd.Parameters.Add("@VisitId", SqlDbType.Int).Value = visitId;
                    var affected = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    if (affected <= 0)
                    {
                        tx.Rollback();
                        return false;
                    }
                }

                if (personId > 0 && visitDate != DateTime.MinValue)
                {
                    const string deleteAttendSql = @"
DELETE FROM dbo.AttendanceCheck_2026
WHERE AttendDate = @AttendDate
  AND PersonId = @PersonId;
";

                    using var cmd = new SqlCommand(deleteAttendSql, conn, tx);
                    cmd.Parameters.Add("@AttendDate", SqlDbType.Date).Value = visitDate;
                    cmd.Parameters.Add("@PersonId", SqlDbType.Int).Value = personId;
                    await cmd.ExecuteNonQueryAsync();
                }

                tx.Commit();
                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        private async Task<List<NewComerRowVM>> GetNewComerListInternalAsync(SqlConnection conn, string keyword, string mode)
        {
            var list = new List<NewComerRowVM>();

            const string sql = @"
DECLARE @Today DATE = CAST(GETDATE() AS DATE);

SELECT
    nc.Id,
    nc.PersonId,
    ISNULL(pm.fullName, nc.ChildName) AS ChildName,
    v.FirstVisitDate,
    nc.ParentName,
    ISNULL(pm.PhoneNumber, nc.Phone) AS Phone,
    pm.bMonth AS BirthMonth,
    pm.bDay AS BirthDay,
    ci.ClassYear,
    ci.ClassBan,
    nc.School,
    nc.PhotoPath,
    nc.IsVoluntary,
    nc.EvangelistName,
    ISNULL(v.VisitCount, 0) AS VisitCount,
    v.LastVisitDate,
    CASE
        WHEN v.FirstVisitDate IS NULL                            THEN N'활성'
        WHEN DATEADD(DAY, 28, v.FirstVisitDate) >= @Today       THEN N'활성'
        WHEN v.VisitCount < 4                                    THEN N'활성'
        ELSE N'완료'
    END AS NewComerStatus
FROM dbo.AttendanceNewComer nc
LEFT JOIN dbo.PersonMaster_2026 pm
    ON pm.PersonId = nc.PersonId
LEFT JOIN dbo.ClassInfo_2026 ci
    ON ci.PersonID = nc.PersonId
OUTER APPLY
(
    SELECT
        COUNT(*)            AS VisitCount,
        MIN(v1.VisitDate)   AS FirstVisitDate,
        MAX(v1.VisitDate)   AS LastVisitDate
    FROM dbo.AttendanceNewComerVisit v1
    WHERE v1.NewComerId = nc.Id
) v
WHERE
    nc.IsDeleted = 0
    AND
    (
        @Keyword = N''
        OR ISNULL(pm.fullName, nc.ChildName) LIKE N'%' + @Keyword + N'%'
        OR nc.ParentName LIKE N'%' + @Keyword + N'%'
        OR ISNULL(pm.PhoneNumber, nc.Phone) LIKE N'%' + @Keyword + N'%'
    )
    AND
    (
        @Mode = N'ALL'
        OR (@Mode = N'ACTIVE'    AND (v.FirstVisitDate IS NULL OR DATEADD(DAY, 28, v.FirstVisitDate) >= @Today OR v.VisitCount < 4))
        OR (@Mode = N'COMPLETED' AND v.FirstVisitDate IS NOT NULL AND DATEADD(DAY, 28, v.FirstVisitDate) < @Today AND v.VisitCount >= 4)
    )
ORDER BY
    v.FirstVisitDate DESC,
    nc.Id DESC;
";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@Keyword", SqlDbType.NVarChar, 100).Value = keyword ?? string.Empty;
            cmd.Parameters.Add("@Mode", SqlDbType.NVarChar, 20).Value = mode ?? "ACTIVE";

            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new NewComerRowVM
                {
                    Id = rdr["Id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["Id"]),
                    PersonId = rdr["PersonId"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["PersonId"]),
                    ChildName = rdr["ChildName"]?.ToString() ?? string.Empty,
                    FirstVisitDate = rdr["FirstVisitDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(rdr["FirstVisitDate"]),
                    ParentName = rdr["ParentName"]?.ToString() ?? string.Empty,
                    Phone = rdr["Phone"]?.ToString() ?? string.Empty,
                    BirthMonth = rdr["BirthMonth"] == DBNull.Value ? null : Convert.ToInt32(rdr["BirthMonth"]),
                    BirthDay = rdr["BirthDay"] == DBNull.Value ? null : Convert.ToInt32(rdr["BirthDay"]),
                    ClassYear = rdr["ClassYear"] == DBNull.Value ? null : Convert.ToInt32(rdr["ClassYear"]),
                    ClassBan = rdr["ClassBan"] == DBNull.Value ? null : Convert.ToInt32(rdr["ClassBan"]),
                    School = rdr["School"] == DBNull.Value ? null : rdr["School"]?.ToString(),
                    PhotoPath = rdr["PhotoPath"] == DBNull.Value ? null : rdr["PhotoPath"]?.ToString(),
                    IsVoluntary = rdr["IsVoluntary"] == DBNull.Value || Convert.ToBoolean(rdr["IsVoluntary"]),
                    EvangelistName = rdr["EvangelistName"] == DBNull.Value ? null : rdr["EvangelistName"]?.ToString(),
                    VisitCount = rdr["VisitCount"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["VisitCount"]),
                    LastVisitDate = rdr["LastVisitDate"] == DBNull.Value ? null : Convert.ToDateTime(rdr["LastVisitDate"]),
                    NewComerStatus = rdr["NewComerStatus"]?.ToString() ?? string.Empty
                });
            }

            return list;
        }

        private async Task<NewComerRowVM?> GetNewComerInfoInternalAsync(SqlConnection conn, int id)
        {
            const string sql = @"
DECLARE @Today DATE = CAST(GETDATE() AS DATE);

SELECT
    nc.Id,
    nc.PersonId,
    ISNULL(pm.fullName, nc.ChildName) AS ChildName,
    v.FirstVisitDate,
    nc.ParentName,
    ISNULL(pm.PhoneNumber, nc.Phone) AS Phone,
    pm.bMonth AS BirthMonth,
    pm.bDay AS BirthDay,
    ci.ClassYear,
    ci.ClassBan,
    nc.School,
    nc.PhotoPath,
    nc.IsVoluntary,
    nc.EvangelistName,
    ISNULL(v.VisitCount, 0) AS VisitCount,
    v.LastVisitDate,
    CASE
        WHEN v.FirstVisitDate IS NULL                            THEN N'활성'
        WHEN DATEADD(DAY, 28, v.FirstVisitDate) >= @Today       THEN N'활성'
        WHEN v.VisitCount < 4                                    THEN N'활성'
        ELSE N'완료'
    END AS NewComerStatus
FROM dbo.AttendanceNewComer nc
LEFT JOIN dbo.PersonMaster_2026 pm
    ON pm.PersonId = nc.PersonId
LEFT JOIN dbo.ClassInfo_2026 ci
    ON ci.PersonID = nc.PersonId
OUTER APPLY
(
    SELECT
        COUNT(*)            AS VisitCount,
        MIN(v1.VisitDate)   AS FirstVisitDate,
        MAX(v1.VisitDate)   AS LastVisitDate
    FROM dbo.AttendanceNewComerVisit v1
    WHERE v1.NewComerId = nc.Id
) v
WHERE
    nc.IsDeleted = 0
    AND nc.Id = @Id;
";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

            using var rdr = await cmd.ExecuteReaderAsync();
            if (await rdr.ReadAsync())
            {
                return new NewComerRowVM
                {
                    Id = rdr["Id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["Id"]),
                    PersonId = rdr["PersonId"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["PersonId"]),
                    ChildName = rdr["ChildName"]?.ToString() ?? string.Empty,
                    FirstVisitDate = rdr["FirstVisitDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(rdr["FirstVisitDate"]),
                    ParentName = rdr["ParentName"]?.ToString() ?? string.Empty,
                    Phone = rdr["Phone"]?.ToString() ?? string.Empty,
                    BirthMonth = rdr["BirthMonth"] == DBNull.Value ? null : Convert.ToInt32(rdr["BirthMonth"]),
                    BirthDay = rdr["BirthDay"] == DBNull.Value ? null : Convert.ToInt32(rdr["BirthDay"]),
                    ClassYear = rdr["ClassYear"] == DBNull.Value ? null : Convert.ToInt32(rdr["ClassYear"]),
                    ClassBan = rdr["ClassBan"] == DBNull.Value ? null : Convert.ToInt32(rdr["ClassBan"]),
                    School = rdr["School"] == DBNull.Value ? null : rdr["School"]?.ToString(),
                    PhotoPath = rdr["PhotoPath"] == DBNull.Value ? null : rdr["PhotoPath"]?.ToString(),
                    IsVoluntary = rdr["IsVoluntary"] == DBNull.Value || Convert.ToBoolean(rdr["IsVoluntary"]),
                    EvangelistName = rdr["EvangelistName"] == DBNull.Value ? null : rdr["EvangelistName"]?.ToString(),
                    VisitCount = rdr["VisitCount"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["VisitCount"]),
                    LastVisitDate = rdr["LastVisitDate"] == DBNull.Value ? null : Convert.ToDateTime(rdr["LastVisitDate"]),
                    NewComerStatus = rdr["NewComerStatus"]?.ToString() ?? string.Empty
                };
            }

            return null;
        }

        private async Task<List<NewComerVisitRowVM>> GetNewComerVisitsInternalAsync(SqlConnection conn, int newComerId)
        {
            var list = new List<NewComerVisitRowVM>();

            const string sql = @"
SELECT
    ROW_NUMBER() OVER (ORDER BY v.VisitDate ASC, v.Id ASC) AS VisitOrder,
    v.Id,
    v.NewComerId,
    v.VisitDate,
    v.InsertDT
FROM dbo.AttendanceNewComerVisit v
WHERE v.NewComerId = @NewComerId
ORDER BY v.VisitDate ASC, v.Id ASC;
";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@NewComerId", SqlDbType.Int).Value = newComerId;

            using var rdr = await cmd.ExecuteReaderAsync();
            while (await rdr.ReadAsync())
            {
                list.Add(new NewComerVisitRowVM
                {
                    Id = rdr["Id"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["Id"]),
                    NewComerId = rdr["NewComerId"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["NewComerId"]),
                    VisitOrder = rdr["VisitOrder"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["VisitOrder"]),
                    VisitDate = rdr["VisitDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(rdr["VisitDate"]),
                    InsertDT = rdr["InsertDT"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(rdr["InsertDT"])
                });
            }

            return list;
        }

        private async Task<int> InsertPersonMasterAsync(
            SqlConnection conn,
            SqlTransaction tx,
            NewComerSaveModel model)
        {
            const string sql = @"
INSERT INTO dbo.PersonMaster_2026
(
    fullName,
    PhoneNumber,
    bMonth,
    bDay
)
VALUES
(
    @fullName,
    @PhoneNumber,
    @bMonth,
    @bDay
);

SELECT CAST(SCOPE_IDENTITY() AS INT);
";

            using var cmd = new SqlCommand(sql, conn, tx);
            cmd.Parameters.Add("@fullName", SqlDbType.NVarChar, 100).Value = model.ChildName.Trim();
            cmd.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 50).Value = model.Phone.Trim();
            cmd.Parameters.Add("@bMonth", SqlDbType.Int).Value = model.BirthMonth!.Value;
            cmd.Parameters.Add("@bDay", SqlDbType.Int).Value = model.BirthDay!.Value;

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        private async Task UpdatePersonMasterAsync(
            SqlConnection conn,
            SqlTransaction tx,
            int personId,
            NewComerSaveModel model)
        {
            const string sql = @"
UPDATE dbo.PersonMaster_2026
SET
    fullName = @fullName,
    PhoneNumber = @PhoneNumber,
    bMonth = @bMonth,
    bDay = @bDay
WHERE PersonId = @PersonId;
";

            using var cmd = new SqlCommand(sql, conn, tx);
            cmd.Parameters.Add("@PersonId", SqlDbType.Int).Value = personId;
            cmd.Parameters.Add("@fullName", SqlDbType.NVarChar, 100).Value = model.ChildName.Trim();
            cmd.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 50).Value = model.Phone.Trim();
            cmd.Parameters.Add("@bMonth", SqlDbType.Int).Value = model.BirthMonth!.Value;
            cmd.Parameters.Add("@bDay", SqlDbType.Int).Value = model.BirthDay!.Value;

            await cmd.ExecuteNonQueryAsync();
        }

        private async Task InsertClassInfoAsync(
            SqlConnection conn,
            SqlTransaction tx,
            int personId,
            int classYear,
            int classBan)
        {
            const string sql = @"
INSERT INTO dbo.ClassInfo_2026
(
    ClassYear,
    ClassBan,
    PersonID
)
VALUES
(
    @ClassYear,
    @ClassBan,
    @PersonID
);
";

            using var cmd = new SqlCommand(sql, conn, tx);
            cmd.Parameters.Add("@ClassYear", SqlDbType.Int).Value = classYear;
            cmd.Parameters.Add("@ClassBan", SqlDbType.Int).Value = classBan;
            cmd.Parameters.Add("@PersonID", SqlDbType.Int).Value = personId;

            await cmd.ExecuteNonQueryAsync();
        }

        private async Task ReplaceClassInfoAsync(
            SqlConnection conn,
            SqlTransaction tx,
            int personId,
            int classYear,
            int classBan)
        {
            const string sql = @"
DELETE FROM dbo.ClassInfo_2026
WHERE PersonID = @PersonID;

INSERT INTO dbo.ClassInfo_2026
(
    ClassYear,
    ClassBan,
    PersonID
)
VALUES
(
    @ClassYear,
    @ClassBan,
    @PersonID
);
";

            using var cmd = new SqlCommand(sql, conn, tx);
            cmd.Parameters.Add("@PersonID", SqlDbType.Int).Value = personId;
            cmd.Parameters.Add("@ClassYear", SqlDbType.Int).Value = classYear;
            cmd.Parameters.Add("@ClassBan", SqlDbType.Int).Value = classBan;

            await cmd.ExecuteNonQueryAsync();
        }

        private async Task<int> InsertNewComerInternalAsync(
            SqlConnection conn,
            SqlTransaction tx,
            NewComerSaveModel model,
            int personId)
        {
            const string sql = @"
INSERT INTO dbo.AttendanceNewComer
(
    PersonId,
    ChildName,
    FirstVisitDate,
    ParentName,
    Phone,
    School,
    IsVoluntary,
    EvangelistName,
    IsDeleted,
    InsertDT,
    UpdateDT
)
VALUES
(
    @PersonId,
    @ChildName,
    @FirstVisitDate,
    @ParentName,
    @Phone,
    @School,
    @IsVoluntary,
    @EvangelistName,
    0,
    @InsertDT,
    @UpdateDT
);

SELECT CAST(SCOPE_IDENTITY() AS INT);
";

            using var cmd = new SqlCommand(sql, conn, tx);
            cmd.Parameters.Add("@PersonId", SqlDbType.Int).Value = personId;
            cmd.Parameters.Add("@ChildName", SqlDbType.NVarChar, 100).Value = model.ChildName.Trim();
            cmd.Parameters.Add("@FirstVisitDate", SqlDbType.Date).Value = model.FirstVisitDate!.Value.Date;
            cmd.Parameters.Add("@ParentName", SqlDbType.NVarChar, 100).Value = model.ParentName.Trim();
            cmd.Parameters.Add("@Phone", SqlDbType.NVarChar, 50).Value = model.Phone.Trim();
            cmd.Parameters.Add("@School", SqlDbType.NVarChar, 200).Value = string.IsNullOrWhiteSpace(model.School) ? DBNull.Value : model.School!;
            cmd.Parameters.Add("@IsVoluntary", SqlDbType.Bit).Value = model.IsVoluntary;
            cmd.Parameters.Add("@EvangelistName", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(model.EvangelistName) ? DBNull.Value : (object)model.EvangelistName!;
            cmd.Parameters.Add("@InsertDT", SqlDbType.DateTime).Value = DateHelper.GetKoreaNow();
            cmd.Parameters.Add("@UpdateDT", SqlDbType.DateTime).Value = DateHelper.GetKoreaNow();

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        private async Task<int> UpdateNewComerInternalAsync(
            SqlConnection conn,
            SqlTransaction tx,
            NewComerSaveModel model,
            int personId)
        {
            const string sql = @"
UPDATE dbo.AttendanceNewComer
SET
    PersonId = @PersonId,
    ChildName = @ChildName,
    FirstVisitDate = @FirstVisitDate,
    ParentName = @ParentName,
    Phone = @Phone,
    School = @School,
    IsVoluntary = @IsVoluntary,
    EvangelistName = @EvangelistName,
    UpdateDT = @UpdateDT
WHERE Id = @Id
  AND IsDeleted = 0;

SELECT @@ROWCOUNT;
";

            using var cmd = new SqlCommand(sql, conn, tx);
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = model.Id;
            cmd.Parameters.Add("@PersonId", SqlDbType.Int).Value = personId;
            cmd.Parameters.Add("@ChildName", SqlDbType.NVarChar, 100).Value = model.ChildName.Trim();
            cmd.Parameters.Add("@FirstVisitDate", SqlDbType.Date).Value = model.FirstVisitDate!.Value.Date;
            cmd.Parameters.Add("@ParentName", SqlDbType.NVarChar, 100).Value = model.ParentName.Trim();
            cmd.Parameters.Add("@Phone", SqlDbType.NVarChar, 50).Value = model.Phone.Trim();
            cmd.Parameters.Add("@School", SqlDbType.NVarChar, 200).Value = string.IsNullOrWhiteSpace(model.School) ? DBNull.Value : model.School!;
            cmd.Parameters.Add("@IsVoluntary", SqlDbType.Bit).Value = model.IsVoluntary;
            cmd.Parameters.Add("@EvangelistName", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(model.EvangelistName) ? DBNull.Value : (object)model.EvangelistName!;
            cmd.Parameters.Add("@UpdateDT", SqlDbType.DateTime).Value = DateHelper.GetKoreaNow();

            var affected = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return affected > 0 ? model.Id : 0;
        }

        private async Task<int> GetPersonIdByNewComerIdAsync(
            SqlConnection conn,
            SqlTransaction tx,
            int newComerId)
        {
            const string sql = @"
SELECT ISNULL(PersonId, 0)
FROM dbo.AttendanceNewComer
WHERE Id = @Id
  AND IsDeleted = 0;
";

            using var cmd = new SqlCommand(sql, conn, tx);
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = newComerId;

            var result = await cmd.ExecuteScalarAsync();
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        public async Task<string?> GetNewComerPhotoPathAsync(int newComerId)
        {
            if (newComerId <= 0)
            {
                return null;
            }

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
SELECT PhotoPath
FROM dbo.AttendanceNewComer
WHERE Id = @Id
  AND IsDeleted = 0;
";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = newComerId;

            var result = await cmd.ExecuteScalarAsync();
            return result == null || result == DBNull.Value ? null : result.ToString();
        }

        public async Task<bool> SaveNewComerPhotoPathAsync(int newComerId, string photoPath, string? userName)
        {
            if (newComerId <= 0 || string.IsNullOrWhiteSpace(photoPath))
            {
                return false;
            }

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            const string sql = @"
UPDATE dbo.AttendanceNewComer
SET
    PhotoPath = @PhotoPath,
    UpdateDT = GETDATE()
WHERE Id = @Id
  AND IsDeleted = 0;

SELECT @@ROWCOUNT;
";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = newComerId;
            cmd.Parameters.Add("@PhotoPath", SqlDbType.NVarChar, 500).Value = photoPath.Trim();

            var affected = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return affected > 0;
        }

        private async Task EnsureFirstVisitExistsAsync(
            SqlConnection conn,
            SqlTransaction tx,
            int newComerId,
            DateTime visitDate)
        {
            const string sql = @"
IF NOT EXISTS
(
    SELECT 1
    FROM dbo.AttendanceNewComerVisit
    WHERE NewComerId = @NewComerId
      AND VisitDate = @VisitDate
)
BEGIN
    INSERT INTO dbo.AttendanceNewComerVisit
    (
        NewComerId,
        VisitDate,
        InsertDT,
        UpdateDT
    )
    VALUES
    (
        @NewComerId,
        @VisitDate,
        @InsertDT,
        @UpdateDT
    );
END
";

            using var cmd = new SqlCommand(sql, conn, tx);
            cmd.Parameters.Add("@NewComerId", SqlDbType.Int).Value = newComerId;
            cmd.Parameters.Add("@VisitDate", SqlDbType.Date).Value = visitDate.Date;
            cmd.Parameters.Add("@InsertDT", SqlDbType.DateTime).Value = DateHelper.GetKoreaNow();
            cmd.Parameters.Add("@UpdateDT", SqlDbType.DateTime).Value = DateHelper.GetKoreaNow();

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> UpdateNewComerInfoAsync(NewComerInfoEditModel model, string? userName)
        {
            if (model == null || model.Id <= 0)
            {
                return false;
            }

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var tx = conn.BeginTransaction();

            try
            {
                var personId = await GetPersonIdByNewComerIdAsync(conn, tx, model.Id);
                if (personId <= 0)
                {
                    tx.Rollback();
                    return false;
                }

                // PersonMaster: 이름·전화번호·생일 업데이트
                {
                    var setParts = new List<string>();
                    if (!string.IsNullOrWhiteSpace(model.ChildName))   setParts.Add("fullName    = @fullName");
                    if (!string.IsNullOrWhiteSpace(model.Phone))       setParts.Add("PhoneNumber = @PhoneNumber");
                    if (model.BirthMonth.HasValue && model.BirthDay.HasValue)
                    {
                        setParts.Add("bMonth = @bMonth");
                        setParts.Add("bDay   = @bDay");
                    }
                    if (setParts.Count > 0)
                    {
                        string pmSql = $"UPDATE dbo.PersonMaster_2026 SET {string.Join(", ", setParts)} WHERE PersonId = @PersonId;";
                        using var pmCmd = new SqlCommand(pmSql, conn, tx);
                        pmCmd.Parameters.Add("@PersonId", SqlDbType.Int).Value = personId;
                        if (!string.IsNullOrWhiteSpace(model.ChildName))
                            pmCmd.Parameters.Add("@fullName",    SqlDbType.NVarChar, 100).Value = model.ChildName.Trim();
                        if (!string.IsNullOrWhiteSpace(model.Phone))
                            pmCmd.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar,  50).Value = model.Phone.Trim();
                        if (model.BirthMonth.HasValue && model.BirthDay.HasValue)
                        {
                            pmCmd.Parameters.Add("@bMonth", SqlDbType.Int).Value = model.BirthMonth.Value;
                            pmCmd.Parameters.Add("@bDay",   SqlDbType.Int).Value = model.BirthDay.Value;
                        }
                        await pmCmd.ExecuteNonQueryAsync();
                    }
                }

                // ClassInfo: 학년/반 교체
                if (model.ClassYear.HasValue && model.ClassBan.HasValue)
                {
                    await ReplaceClassInfoAsync(conn, tx, personId, model.ClassYear.Value, model.ClassBan.Value);
                }

                // AttendanceNewComer: 이름·전화번호·학교·자진출석·전도자 업데이트
                const string ncSql = @"
UPDATE dbo.AttendanceNewComer
SET ChildName      = CASE WHEN @ChildName IS NOT NULL THEN @ChildName ELSE ChildName END,
    Phone          = CASE WHEN @Phone IS NOT NULL THEN @Phone ELSE Phone END,
    School         = @School,
    IsVoluntary    = @IsVoluntary,
    EvangelistName = @EvangelistName,
    UpdateDT       = @UpdateDT
WHERE Id        = @Id
  AND IsDeleted = 0;

SELECT @@ROWCOUNT;
";
                using (var ncCmd = new SqlCommand(ncSql, conn, tx))
                {
                    ncCmd.Parameters.Add("@Id", SqlDbType.Int).Value = model.Id;
                    ncCmd.Parameters.Add("@ChildName", SqlDbType.NVarChar, 100).Value =
                        string.IsNullOrWhiteSpace(model.ChildName) ? DBNull.Value : (object)model.ChildName.Trim();
                    ncCmd.Parameters.Add("@Phone", SqlDbType.NVarChar, 50).Value =
                        string.IsNullOrWhiteSpace(model.Phone) ? DBNull.Value : (object)model.Phone.Trim();
                    ncCmd.Parameters.Add("@School", SqlDbType.NVarChar, 200).Value =
                        string.IsNullOrWhiteSpace(model.School) ? DBNull.Value : (object)model.School!;
                    ncCmd.Parameters.Add("@IsVoluntary", SqlDbType.Bit).Value = model.IsVoluntary;
                    ncCmd.Parameters.Add("@EvangelistName", SqlDbType.NVarChar, 100).Value =
                        string.IsNullOrWhiteSpace(model.EvangelistName) ? DBNull.Value : (object)model.EvangelistName!;
                    ncCmd.Parameters.Add("@UpdateDT", SqlDbType.DateTime).Value = DateHelper.GetKoreaNow();

                    var affected = Convert.ToInt32(await ncCmd.ExecuteScalarAsync());
                    if (affected <= 0)
                    {
                        tx.Rollback();
                        return false;
                    }
                }

                tx.Commit();
                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        private async Task EnsureAttendanceCheckExistsAsync(
            SqlConnection conn,
            SqlTransaction tx,
            int personId,
            DateTime attendDate)
        {
            const string sql = @"
IF NOT EXISTS
(
    SELECT 1
    FROM dbo.AttendanceCheck_2026
    WHERE AttendDate = @AttendDate
      AND PersonId = @PersonId
)
BEGIN
    INSERT INTO dbo.AttendanceCheck_2026
    (
        AttendDate,
        PersonId,
        IsPresent
    )
    VALUES
    (
        @AttendDate,
        @PersonId,
        1
    );
END
";

            using var cmd = new SqlCommand(sql, conn, tx);
            cmd.Parameters.Add("@AttendDate", SqlDbType.Date).Value = attendDate.Date;
            cmd.Parameters.Add("@PersonId", SqlDbType.Int).Value = personId;

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> GetEvangelismCountByUserIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return 0;

            const string sql = @"
SELECT COUNT(*)
FROM [dbo].[AttendanceNewComer] nc
INNER JOIN [dbo].[view_PersonMaster_2026] pm ON nc.EvangelistName = pm.fullName
WHERE pm.UserId = @UserId
  AND nc.IsDeleted = 0";

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@UserId", SqlDbType.NVarChar, 450).Value = userId;
            var result = await cmd.ExecuteScalarAsync();
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }
    }
}