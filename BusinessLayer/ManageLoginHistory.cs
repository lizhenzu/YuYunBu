using BulletinBoard.Common;
using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace BulletinBoard.BusinessLayer
{
    public class ManageLoginHistory
    {
        public void AddLoginHistory(LoginHistory loginHistory)
        {
            ControlLoginHistory controlLoginHistory = new ControlLoginHistory();
            controlLoginHistory.AddLoginHistory(loginHistory.ControllerName, loginHistory.ActionName, loginHistory.UserName, loginHistory.requestIP);            
        }

        public List<LoginHistoryData> getLoginHist111ory()
        {
            ControlLoginHistory controlLoginHistory = new ControlLoginHistory();
            DataSet ds = controlLoginHistory.fetchLoginHistory();
            List<LoginHistoryData> value = ds.Tables[0].ToList<LoginHistoryData>();
            return value;
        }
        public List<LoginHistoryData> getLoginHistory(IWebHostEnvironment _hostingEnvironment)
        {
            ControlLoginHistory controlLoginHistory = new ControlLoginHistory();
            DataSet ds = controlLoginHistory.fetchLoginHistory();

            var profilePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", "profile");

            List<LoginHistoryData> value = new List<LoginHistoryData>();

            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DataRow dr = ds.Tables[0].Rows[i];

                LoginHistoryData _LoginHistoryData = new LoginHistoryData();

                _LoginHistoryData.sequenceNo = long.Parse(dr["sequenceNo"].ToString());

                _LoginHistoryData.UserName = dr["UserName"].ToString() as string;
                _LoginHistoryData.UserId = dr["UserId"].ToString() as string;
                _LoginHistoryData.FullName = dr["FullName"].ToString() as string;
                _LoginHistoryData.ClassYear = int.Parse(dr["ClassYear"].ToString());
                _LoginHistoryData.ClassBan = int.Parse(dr["ClassBan"].ToString());
                _LoginHistoryData.DC = int.Parse(dr["DC"].ToString());

                _LoginHistoryData.minInsertDT = DateTime.Parse(dr["minInsertDT"].ToString());
                _LoginHistoryData.maxInsertDT = DateTime.Parse(dr["maxInsertDT"].ToString());


                _LoginHistoryData.RoleCode = dr["RoleCode"].ToString() as string;
                _LoginHistoryData.RoleNM = dr["RoleNM"].ToString() as string;

                string filePath = Path.Combine(profilePath, $"{_LoginHistoryData.UserId}.png");
                         

                if (System.IO.File.Exists(filePath))
                {
                    _LoginHistoryData.ProfilePicturePath = $"/images/profile/{_LoginHistoryData.UserId}.png";
                }
                else
                {
                    _LoginHistoryData.ProfilePicturePath = ConstatsCommon.representFaceImage;
                }
                value.Add(_LoginHistoryData);
            }
            return value;
        }

        public void DeleteLoginHistory(string UserName)
        {
            ControlLoginHistory controlLoginHistory = new ControlLoginHistory();
            controlLoginHistory.DeleteLoginHistory(UserName);
        } 
    }
}
