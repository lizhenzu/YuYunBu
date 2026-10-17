using BulletinBoard.Common;
using System;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.IO;
using BulletinBoard.Helpers;

namespace BulletinBoard.DataAccessLayer
{
    public class ControlHumanResource
    {
        /// <summary>
        /// PhoneNumber2, , 
        /// </summary>
        public DataSet fetchgetUserInfos(string UserId, string UserName, int PersonId, int ClassYear, int ClassBan, string BanMode, string DeptCode, RankQueryOption RankQueryOptionData)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(fetchgetUserInfoSubQuery());

            sb.AppendFormat(" where 1 = 1            ");
         
            if (!string.IsNullOrEmpty(UserId))
            {
                sb.AppendFormat(" and UserId = '{0}' ", UserId); sb.Append(Environment.NewLine);
            }
            else if (!string.IsNullOrEmpty(UserName))
            {
                sb.AppendFormat(" and a.UserName = '{0}' ", UserName); sb.Append(Environment.NewLine);
            }
            else if (!string.IsNullOrEmpty(BanMode) && BanMode == "IndexTeacher")
            {
                sb.AppendFormat(" and RoleCode in ('T1002','T1003','T1004') ", ClassBan); sb.Append(Environment.NewLine);
            }
            else if (ClassYear > 0 && ClassBan > 0)
            {
                sb.AppendFormat(" and ClassYear = {0} ", ClassYear); sb.Append(Environment.NewLine);
                sb.AppendFormat(" and ClassBan  = {0} ", ClassBan); sb.Append(Environment.NewLine);
            }
            else if (ClassYear > 0)
            {
                sb.AppendFormat(" and ClassYear = {0} ", ClassYear); sb.Append(Environment.NewLine);
            }
            else if (RankQueryOptionData == RankQueryOption.StudentOnly)
            {
                sb.AppendFormat(" and RoleCode in ('T1001') ", ClassBan); sb.Append(Environment.NewLine);
            }
            else if (RankQueryOptionData == RankQueryOption.TeacherOnly)
            {
                sb.AppendFormat(" and RoleCode in ( "); sb.Append(Environment.NewLine);
                sb.AppendFormat(" select 'T1002' RoleCode "); sb.Append(Environment.NewLine);
                sb.AppendFormat(" union "); sb.Append(Environment.NewLine);
                sb.AppendFormat(" select 'T1003' RoleCode "); sb.Append(Environment.NewLine);
                sb.AppendFormat(" union "); sb.Append(Environment.NewLine);
                sb.AppendFormat(" select RoleCode from [dbo].[DeptTeacherMap_2026]  "); sb.Append(Environment.NewLine);
                sb.AppendFormat(" ) "); sb.Append(Environment.NewLine);
            }
            else if (RankQueryOptionData == RankQueryOption.All)
            {
                sb.AppendFormat(" and RoleCode in ( "); sb.Append(Environment.NewLine);
                sb.AppendFormat(" select 'T1001' RoleCode "); sb.Append(Environment.NewLine);
                sb.AppendFormat(" union "); sb.Append(Environment.NewLine);
                sb.AppendFormat(" select 'T1002' RoleCode "); sb.Append(Environment.NewLine);
                sb.AppendFormat(" union "); sb.Append(Environment.NewLine);
                sb.AppendFormat(" select 'T1003' RoleCode "); sb.Append(Environment.NewLine);
                sb.AppendFormat(" union "); sb.Append(Environment.NewLine);
                sb.AppendFormat(" select RoleCode from [dbo].[DeptTeacherMap_2026]  "); sb.Append(Environment.NewLine);
                sb.AppendFormat(" ) "); sb.Append(Environment.NewLine);
            }
            else if (RankQueryOptionData == RankQueryOption.UnClassify)
            {
                sb.AppendFormat(" and isnull(DeptCode,'') = '' ", ClassBan); sb.Append(Environment.NewLine);
            }

            if (!string.IsNullOrEmpty(DeptCode))
            {
                sb.AppendFormat($" and DeptCode = '{DeptCode}'  "); sb.Append(Environment.NewLine);
            }

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }
 

        public DataSet fetchApplyEventList(int EventId)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(fetchgetUserInfoSubQuery());

            sb.AppendFormat(" where 1 = 1            ");

            sb.AppendFormat(" and a.UserId in (               "); sb.Append(Environment.NewLine);
            sb.AppendFormat("                 select UserId  "); sb.Append(Environment.NewLine);
            sb.AppendFormat("                 from [dbo].[EventApplyInfo] "); sb.Append(Environment.NewLine);
            if (EventId > 0)
            {
                sb.AppendFormat("                 where EventId = {0} ", EventId); sb.Append(Environment.NewLine);
            }
            sb.AppendFormat("                ) "); sb.Append(Environment.NewLine);
            if (EventId > 0)
            {
                sb.AppendFormat(" and c.EventId = {0} ", EventId); sb.Append(Environment.NewLine);
            }

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }

        private string fetchgetUserInfoSubQuery()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" select top 1000 ROW_NUMBER() OVER(ORDER BY FullName )  sequenceNo ,");
            sb.AppendFormat("  UserId      , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  UserName  , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  PersonId    , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  Email       , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  FirstName   , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  LastName    , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  FullName    , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(bMonth,0)  bMonth    , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(bDay  ,0)  bDay      , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull([birthDay_PublishYN],'') [birthDay_PublishYN]  , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  readBibleAllCount  , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(ClassYear  ,0)  ClassYear      , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  isnull(ClassBan  ,0)  ClassBan      , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  PhoneNumber , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  PhoneNumber2 , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  DeptCode     , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  HomeAaddress , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  school       , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  sex         , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  DeptNM      , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  RoleCode    , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  RoleNM      , "); sb.Append(Environment.NewLine);
 
            sb.AppendFormat("  ''       ProfilePicture       "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from [dbo].[view_UserCompositeInfo_2026]   "); sb.Append(Environment.NewLine);
 

            return sb.ToString();
        }

        public DataSet fetchgetClassInfos()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" select distinct   ");
            sb.AppendFormat("  0  sequenceNo  , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  '' UserId      , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  '' UserName    , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  0  PersonId    , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  '' Email       , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  '' FirstName   , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  '' LastName    , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  '' FullName    , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  0  bMonth      , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  0  bDay        , "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" birthDay_PublishYN , "); sb.Append(Environment.NewLine);

            sb.AppendFormat("  0  CountD       , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  readBibleAllCount      , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("     ClassYear   , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("     ClassBan    , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  '' PhoneNumber , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  '' PhoneNumber2, "); sb.Append(Environment.NewLine);

            sb.AppendFormat("  '' HomeAaddress , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  '' school       , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  '' sex       , "); sb.Append(Environment.NewLine);

            sb.AppendFormat("  '' DeptCode     , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  '' DeptNM       , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  '' RoleCode     , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  0 DC_Total      , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  '' RoleNM       , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  GETDATE() insertDT , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("  '' ProfilePicturePath   "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" from [dbo].[view_UserCompositeInfo_2026]  "); sb.Append(Environment.NewLine);

            sb.AppendFormat(" where 1 = 1            ");
            sb.AppendFormat(" and   ClassYear > 0    ");
            sb.AppendFormat(" and   ClassBan  > 0    ");
            sb.AppendFormat(" ORDER BY ClassYear,  ClassBan        ");

            DataSet ds = DataAccessModule.commonQuery(sb.ToString());
            return ds;
        }


        public void CreateHumanResource(string MessageText, string FromWhom, string ToWhom)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" insert into [dbo].[HumanResource] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" ("); sb.Append(Environment.NewLine);
            sb.AppendFormat("     MessageText, ToWhom, FromWhom "); sb.Append(Environment.NewLine);
            sb.AppendFormat("   )"); sb.Append(Environment.NewLine);

            // ⚠️ 원본에 따옴표 오류가 있어 보이므로 최소한 정상 SQL 형태로만 수정
            sb.AppendFormat(" values('{0}','{1}','{2}')", MessageText, ToWhom, FromWhom); sb.Append(Environment.NewLine);

            DataAccessModule.executeNonQuery(sb.ToString());
        }


        public void UpdateHumanResource(int Id, string MessageText, string FromWhom, string ToWhom)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" update [dbo].[HumanResource] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set [MessageText]  = '{0}', ", MessageText); sb.Append(Environment.NewLine);
            sb.AppendFormat("    [ToWhom]        = '{0}', ", ToWhom); sb.Append(Environment.NewLine);
            sb.AppendFormat("    [FromWhom]      = '{0}', ", FromWhom); sb.Append(Environment.NewLine);
            sb.AppendFormat("    [updateDT]      = '{0:yyyy-MM-dd HH:mm:ss}', ", SharedComm.GetKoreaNow());
            sb.AppendFormat("    [InsertDT]      = '{0:yyyy-MM-dd HH:mm:ss}'  ", SharedComm.GetKoreaNow());
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }


        public void UpdateHitHumanResource(string ToWhom)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" update [dbo].[HumanResource] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" set [Visited]  = [Visited] + 1 , "); sb.Append(Environment.NewLine);
            sb.AppendFormat("     [updateDT]  = '{0:yyyy-MM-dd HH:mm:ss}' ", SharedComm.GetKoreaNow());
            sb.AppendFormat(" where [ToWhom] =  '{0}' ", ToWhom); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }

        public async Task<byte[]> RetrieveProfilePictureAsync(string userId)
        {
            using (SqlConnection conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                await conn.OpenAsync();

                string sql = "SELECT [ProfilePicture] FROM [Identity].[User] WHERE [Id] = @UserId";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = userId });

                    var result = await cmd.ExecuteScalarAsync();
                    return result as byte[];
                }
            }
        }


        public async Task SaveProfileEtcAsync(string UserId, string LastName, string FirstName, string PhoneNumber)
        {
            if (string.IsNullOrEmpty(UserId))
                return;

            using (SqlConnection conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                await conn.OpenAsync();

                string sql = @"
                    UPDATE [Identity].[User]
                    SET [LastName]       = @LastName       ,
                        [FirstName]      = @FirstName      , 
                        [PhoneNumber]    = @PhoneNumber 
                    WHERE [Id] = @UserId";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@LastName", SqlDbType.NVarChar) { Value = (object)LastName ?? DBNull.Value });
                    cmd.Parameters.Add(new SqlParameter("@FirstName", SqlDbType.NVarChar) { Value = (object)FirstName ?? DBNull.Value });
                    cmd.Parameters.Add(new SqlParameter("@PhoneNumber", SqlDbType.NVarChar) { Value = (object)PhoneNumber ?? DBNull.Value });
                    cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = UserId });

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // =========================================================
        // ✅ 핵심 변경: PersonMaster_2026 은 UserId 컬럼이 없으므로
        //     view_PersonMaster_2026 에서 UserId로 PersonId를 찾아서
        //     PersonId로 PersonMaster_2026 을 Update/Insert 한다.
        // =========================================================

        /// <summary>
        /// view_PersonMaster_2026 에서 UserId로 PersonId를 조회
        /// </summary>
        private static async Task<int?> GetPersonIdByUserIdAsync(SqlConnection conn, SqlTransaction? tx, string userId)
        {
            string sql = @"
                SELECT TOP 1 PersonId
                FROM [dbo].[view_PersonMaster_2026]
                WHERE [UserId] = @UserId";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                object? result = await cmd.ExecuteScalarAsync();
                if (result == null || result == DBNull.Value) return null;
                return Convert.ToInt32(result);
            }
        }

        /// <summary>
        /// Identity.User에서 UserId로 FullName(LastName+FirstName) 조회
        /// </summary>
        private static async Task<string?> GetFullNameByUserIdAsync(SqlConnection conn, SqlTransaction? tx, string userId)
        {
            string sql = @"
                SELECT TOP 1 (ISNULL([LastName],'') + ISNULL([FirstName],'')) AS FullName
                FROM [Identity].[User]
                WHERE [Id] = @UserId";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                object? result = await cmd.ExecuteScalarAsync();
                if (result == null || result == DBNull.Value) return null;

                var fullName = Convert.ToString(result);
                return string.IsNullOrWhiteSpace(fullName) ? null : fullName;
            }
        }

        /// <summary>
        /// (필요 시) PersonMaster_2026 신규 생성용: FullName 기반 insert
        /// - 테이블 구조를 정확히 모르므로, "존재 가능성이 높은 컬럼들"만 사용
        /// - 최소: fullName + PhoneNumber + bMonth + bDay + insertDT
        /// </summary>
        private static async Task InsertPersonMaster2026Async(
            SqlConnection conn,
            SqlTransaction? tx,
            string fullName,
            string? phoneNumber,
            int bMonth,
            int bDay,
            string? birthDay_PublishYN = null)
        {
            // birthDay_PublishYN 컬럼이 있다고 가정(뷰에도 존재)
            // 없으면 이 컬럼 라인을 제거해야 합니다.
            string sql = @$"
                INSERT INTO [dbo].[PersonMaster_2026]
                    ([fullName], [PhoneNumber], [bMonth], [bDay], [birthDay_PublishYN], [insertDT])
                VALUES
                    (@fullName, @PhoneNumber, @bMonth, @bDay, @birthDay_PublishYN, @insertDT )";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@fullName", fullName);
                cmd.Parameters.AddWithValue("@PhoneNumber", (object?)phoneNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@bMonth", bMonth);
                cmd.Parameters.AddWithValue("@bDay", bDay);
                cmd.Parameters.AddWithValue("@birthDay_PublishYN", (object?)birthDay_PublishYN ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@insertDT", SharedComm.GetKoreaNow());

                await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// PersonMaster_2026 저장(전화번호, 생일)
        /// - UserId로 view에서 PersonId 조회 → PersonId로 Update
        /// - 없으면 Identity.User에서 FullName 얻어 Insert → 다시 PersonId 얻어 Update
        /// </summary>
        public async Task SavePersonMasterAsync(string UserId, string PhoneNumber, int bMonth, int bDay)
        {
            if (string.IsNullOrWhiteSpace(UserId)) return;

            using (SqlConnection conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                await conn.OpenAsync();

                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        int? personId = await GetPersonIdByUserIdAsync(conn, tx, UserId);

                        if (!personId.HasValue)
                        {
                            // view에 매핑이 없으면 신규 생성 시도(FullName 기반)
                            string? fullName = await GetFullNameByUserIdAsync(conn, tx, UserId);
                            if (!string.IsNullOrWhiteSpace(fullName))
                            {
                                await InsertPersonMaster2026Async(conn, tx, fullName!, PhoneNumber, bMonth, bDay);
                            }

                            // Insert 후 다시 매핑 시도
                            personId = await GetPersonIdByUserIdAsync(conn, tx, UserId);
                        }

                        // personId가 확보되면 Update
                        if (personId.HasValue)
                        {
                            string sqlUpdate = @$"
                                UPDATE [dbo].[PersonMaster_2026]
                                SET [bMonth]      = @bMonth,
                                    [bDay]        = @bDay,
                                    [PhoneNumber] = @PhoneNumber,
                                    [updateDT]    = @updateDT
                                WHERE [PersonId]  = @PersonId";

                            using (SqlCommand cmdUpdate = new SqlCommand(sqlUpdate, conn, tx))
                            {
                                cmdUpdate.Parameters.AddWithValue("@PersonId", personId.Value);
                                cmdUpdate.Parameters.AddWithValue("@PhoneNumber", (object?)PhoneNumber ?? DBNull.Value);
                                cmdUpdate.Parameters.AddWithValue("@bMonth", bMonth);
                                cmdUpdate.Parameters.AddWithValue("@bDay", bDay);
                                cmdUpdate.Parameters.AddWithValue("@updateDT", SharedComm.GetKoreaNow());

                                await cmdUpdate.ExecuteNonQueryAsync();
                            }
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// PersonMaster_2026 저장(공개여부 + 생일)
        /// - UserId로 view에서 PersonId 조회 → PersonId로 Update
        /// - 없으면 Identity.User에서 FullName 얻어 Insert → 다시 PersonId 얻어 Update
        /// </summary>
        public async Task SavePersonMasterBirthAsync(string UserId, string birthDay_PublishYN, int bMonth, int bDay)
        {
            if (string.IsNullOrWhiteSpace(UserId)) return;

            using (SqlConnection conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                await conn.OpenAsync();

                using (SqlTransaction tx = conn.BeginTransaction())
                {
                    try
                    {
                        int? personId = await GetPersonIdByUserIdAsync(conn, tx, UserId);

                        if (!personId.HasValue)
                        {
                            // view에 매핑이 없으면 신규 생성 시도(FullName 기반)
                            string? fullName = await GetFullNameByUserIdAsync(conn, tx, UserId);
                            if (!string.IsNullOrWhiteSpace(fullName))
                            {
                                await InsertPersonMaster2026Async(conn, tx, fullName!, null, bMonth, bDay, birthDay_PublishYN);
                            }

                            // Insert 후 다시 매핑 시도
                            personId = await GetPersonIdByUserIdAsync(conn, tx, UserId);
                        }

                        // personId가 확보되면 Update
                        if (personId.HasValue)
                        {
                            string sqlUpdate = @$"
                                UPDATE [dbo].[PersonMaster_2026]
                                SET [birthDay_PublishYN] = @birthDay_PublishYN,
                                    [bMonth]             = @bMonth,
                                    [bDay]               = @bDay,
                                    [updateDT]           = @updateDT
                                WHERE [PersonId]         = @PersonId";

                            using (SqlCommand cmdUpdate = new SqlCommand(sqlUpdate, conn, tx))
                            {
                                cmdUpdate.Parameters.AddWithValue("@PersonId", personId.Value);
                                cmdUpdate.Parameters.AddWithValue("@birthDay_PublishYN", (object?)birthDay_PublishYN ?? DBNull.Value);
                                cmdUpdate.Parameters.AddWithValue("@bMonth", bMonth);
                                cmdUpdate.Parameters.AddWithValue("@bDay", bDay);
                                cmdUpdate.Parameters.AddWithValue("@updateDT", SharedComm.GetKoreaNow());

                                await cmdUpdate.ExecuteNonQueryAsync();
                            }
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }
 

        public async Task SaveClassInfoAsync(string UserId, int ClassYear, int ClassBan)
        {
            using (SqlConnection conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                await conn.OpenAsync();

                string sqlUpdate = @$"
                                    UPDATE [ClassInfo]
                                    SET [ClassYear] = @ClassYear,
                                        [ClassBan]  = @ClassBan,
                                        [updateDT]  = @updateDT
                                    WHERE [UserId]  = @UserId";

                using (SqlCommand cmdUpdate = new SqlCommand(sqlUpdate, conn))
                {
                    cmdUpdate.Parameters.AddWithValue("@UserId", UserId);
                    cmdUpdate.Parameters.AddWithValue("@ClassYear", ClassYear);
                    cmdUpdate.Parameters.AddWithValue("@ClassBan", ClassBan);
                    cmdUpdate.Parameters.AddWithValue("@updateDT", SharedComm.GetKoreaNow());

                    int rowsAffected = await cmdUpdate.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        string sqlInsert = @$"
                                            INSERT INTO [ClassInfo] ([UserId], [ClassYear], [ClassBan])
                                            VALUES (@UserId, @ClassYear, @ClassBan)";

                        using (SqlCommand cmdInsert = new SqlCommand(sqlInsert, conn))
                        {
                            cmdInsert.Parameters.AddWithValue("@UserId", UserId);
                            cmdInsert.Parameters.AddWithValue("@ClassYear", ClassYear);
                            cmdInsert.Parameters.AddWithValue("@ClassBan", ClassBan);

                            await cmdInsert.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
        }

        public async Task DeleteDeptRoleUserAssocAsync(string UserId)
        {
            using (SqlConnection conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                await conn.OpenAsync();

                string sqlDelete = @$"
                            DELETE FROM [DeptRoleUserAssoc]
                            WHERE [UserId] = @UserId";

                using (SqlCommand cmdDelete = new SqlCommand(sqlDelete, conn))
                {
                    cmdDelete.Parameters.AddWithValue("@UserId", UserId);
                    await cmdDelete.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteClassInfoAsync(string UserId)
        {
            using (SqlConnection conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                await conn.OpenAsync();

                string sqlDelete = @$"
                            DELETE FROM [ClassInfo]
                            WHERE [UserId] = @UserId";

                using (SqlCommand cmdDelete = new SqlCommand(sqlDelete, conn))
                {
                    cmdDelete.Parameters.AddWithValue("@UserId", UserId);
                    await cmdDelete.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task SaveDeptRoleUserAssocAsync(string UserId, string DeptCode, string RoleCode)
        {
            using (SqlConnection conn = new SqlConnection(DataAccessModule.connectionString_Server))
            {
                await conn.OpenAsync();

                string sqlUpdate = @$"
                                    UPDATE [DeptRoleUserAssoc]
                                    SET [DeptCode] = @DeptCode,
                                        [RoleCode] = @RoleCode,
                                        [updateDT] = @updateDT
                                    WHERE [UserId] = @UserId";

                using (SqlCommand cmdUpdate = new SqlCommand(sqlUpdate, conn))
                {
                    cmdUpdate.Parameters.AddWithValue("@UserId", UserId);
                    cmdUpdate.Parameters.AddWithValue("@DeptCode", DeptCode);
                    cmdUpdate.Parameters.AddWithValue("@RoleCode", RoleCode);
                    cmdUpdate.Parameters.AddWithValue("@updateDT", SharedComm.GetKoreaNow());

                    int rowsAffected = await cmdUpdate.ExecuteNonQueryAsync();

                    if (rowsAffected == 0)
                    {
                        string sqlInsert = @$"
                                            INSERT INTO [DeptRoleUserAssoc] ([UserId], [DeptCode], [RoleCode])
                                            VALUES (@UserId, @DeptCode, @RoleCode)";

                        using (SqlCommand cmdInsert = new SqlCommand(sqlInsert, conn))
                        {
                            cmdInsert.Parameters.AddWithValue("@UserId", UserId);
                            cmdInsert.Parameters.AddWithValue("@DeptCode", DeptCode);
                            cmdInsert.Parameters.AddWithValue("@RoleCode", RoleCode);

                            await cmdInsert.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
        }


        public async Task SaveProfilePictureAsync(string Id, byte[] imageBytes, string filePath)
        {
            // DB 저장 대신 파일로 저장(원본 유지)
            SaveImageAsPng(imageBytes, filePath);
            await Task.CompletedTask;
        }

        static void SaveImageAsPng(byte[] imageBytes, string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(imageBytes, 0, imageBytes.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving image: " + ex.Message);
            }
        }


        public void DeleteHumanResource(int Id)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" delete from [dbo].[HumanResource] "); sb.Append(Environment.NewLine);
            sb.AppendFormat(" where [Id] =  {0} ", Id); sb.Append(Environment.NewLine);
            DataAccessModule.executeNonQuery(sb.ToString());
        }
    }
}
