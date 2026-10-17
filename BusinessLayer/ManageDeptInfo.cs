using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BulletinBoard.BusinessLayer
{
    public class ManageDeptInfo
    {

        public List<DeptRoleUserAssoc> getDeptRoleUserAssococ(string UserId)
        {
            ControlDeptInfo controlDeptInfo = new ControlDeptInfo();
            DataSet ds = controlDeptInfo.fetchDeptRoleUserAssococ(UserId);
            List<DeptRoleUserAssoc> value = ds.Tables[0].ToList<DeptRoleUserAssoc>();
            return value;
        }
        public List<PointByUserName> getDeptInfo(string UserId)
        {
            ControlDeptInfo controlDeptInfo = new ControlDeptInfo();
            DataSet ds = controlDeptInfo.fetchDeptInfo(UserId);
            List<PointByUserName> value = ds.Tables[0].ToList<PointByUserName>();
            return value;
        }

        public List<CodeData> getCodeData(string MajorCD)
        {
            ControlDeptInfo controlDeptInfo = new ControlDeptInfo();
            DataSet ds = controlDeptInfo.fetchCodeData(MajorCD);
            List<CodeData> value = ds.Tables[0].ToList<CodeData>();
            return value;
        }

      
    }
}
