using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;

namespace BulletinBoard.BusinessLayer
{
    /// <summary>공통 코드(dbo.CodeData) 읽기 — 예: 역할 코드 T001 (예전 ManageDeptInfo 에서 부서 부분을 지우고 남긴 것, 2026-11-09)</summary>
    public class ManageCodeData
    {
        public List<CodeData> getCodeData(string MajorCD)
        {
            DataSet ds = new ControlCodeData().fetchCodeData(MajorCD);
            if (ds.Tables.Count == 0) return new List<CodeData>();
            return ds.Tables[0].ToList<CodeData>();
        }
    }
}
