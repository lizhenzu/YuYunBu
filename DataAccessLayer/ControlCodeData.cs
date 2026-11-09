using System.Data;
using BulletinBoard.Common;

namespace BulletinBoard.DataAccessLayer
{
    /// <summary>공통 코드(dbo.CodeData) — 예전 ControlDeptInfo 에서 부서 부분을 지우고 남긴 것 (2026-11-09)</summary>
    public class ControlCodeData
    {
        public DataSet fetchCodeData(string MajorCD)
        {
            string code = (MajorCD ?? "").Replace("'", "''");
            return DataAccessModule.commonQuery($"select MinorCD, MinorNM from [dbo].[CodeData] where MajorCD = N'{code}' order by [MinorCD]");
        }
    }
}
