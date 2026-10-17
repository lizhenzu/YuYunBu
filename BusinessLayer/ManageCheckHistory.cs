using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;
using System.Globalization;

namespace BulletinBoard.BusinessLayer
{
    public class ManageCheckHistory
    {
         
        public List<checkAllTable> getALLTable()
        {
            ControlCheckHistory controlCheckHistory = new ControlCheckHistory();
            DataSet ds = controlCheckHistory.fetchALLTable();

            DataView dv = ds.Tables[0].DefaultView;
            dv.Sort = "CreatedDate DESC";

            // 정렬된 DataView를 DataTable로 변환
            DataTable sortedDt = dv.ToTable();

            DataSet ds1 = new DataSet();
            ds1.Tables.Add(sortedDt);



            List<checkAllTable> value = ds1.Tables[0].ToList<checkAllTable>();


         

            return value;
        }
 
    }
}
