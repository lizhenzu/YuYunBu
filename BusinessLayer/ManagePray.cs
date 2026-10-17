using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace BulletinBoard.BusinessLayer
{
    public class ManagePray
    {

        public int ConvertToInt(DataRow dr, string keyData)
        {
            object ObjectData = dr[keyData];
            int data;

            if (ObjectData != null && int.TryParse(ObjectData.ToString(), out data))
            {
                return data;
            }
            else
            {
                return 0;
            }
        }

        public DateTime ConvertToDate(DataRow dr, string keyData)
        {
            object ObjectData = dr[keyData];
            DateTime data;

            if (ObjectData != null && DateTime.TryParse(ObjectData.ToString(), out data))
            {
                return data;
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        public string ConvertToString(DataRow dr, string keyData)
        {
            object nameObj = dr[keyData];
            return (nameObj != null && nameObj != DBNull.Value ? nameObj.ToString() : "");

        }

        public void CreatePrayHistory(string personPraying, string prayerRecipient)
        {
            ControlPray controlPray = new ControlPray();

            controlPray.CreatePrayHistory(personPraying, prayerRecipient);
        }

        public List<TodayPray> TodayPray(bool today)
        {
            ControlPray controlPray = new ControlPray();
            DataSet ds = controlPray.TodayPray(today);
            List<TodayPray> value = ds.Tables[0].ToList<TodayPray>();
            return value;
        }
        public List<TodayPray> TodayPrayByID(int PrayID)
        {
            ControlPray controlPray = new ControlPray();
            DataSet ds = controlPray.TodayPrayByID(PrayID);
            List<TodayPray> value = ds.Tables[0].ToList<TodayPray>();
            return value;
        }

        public void UpdateTodayPray(int PrayID, string Subject, string PrayTEXTData, string BibleText, string BibleTextData, string publishYN)
        {
            ControlPray controlPray = new ControlPray();

            controlPray.UpdateTodayPray(PrayID, Subject, PrayTEXTData, BibleText, BibleTextData, publishYN);
        }

        public List<UserInfosPray> getPrayHistory(IWebHostEnvironment _hostingEnvironment, int ClassYear, int ClassBan, string PersonPraying)
        {
            ControlPray controlPray = new ControlPray();
            DataSet ds = controlPray.fetchPrayHistory(ClassYear, ClassBan, PersonPraying);
            //     List<UserInfos> value = ds.Tables[0].ToList<UserInfos>();

            List<UserInfosPray> value = new List<UserInfosPray>();

            var profilePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", "profile");

            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DataRow dr = ds.Tables[0].Rows[i];

                UserInfosPray data = new UserInfosPray();

                data.sequenceNo = long.Parse(dr["sequenceNo"].ToString());

                data.UserId = dr["UserId"].ToString() as string;
                data.UserId = dr["UserId"].ToString() as string;
                data.Email = dr["Email"].ToString() as string;
                data.FullName = dr["FullName"].ToString() as string;
                data.UserName = dr["UserName"].ToString() as string;
                data.FirstName = dr["FirstName"].ToString() as string;
                data.LastName = dr["LastName"].ToString() as string;
                data.PhoneNumber = dr["PhoneNumber"].ToString() as string;
                data.ClassYear = int.Parse(dr["ClassYear"].ToString());
                data.ClassBan = int.Parse(dr["ClassBan"].ToString());
                data.bMonth = int.Parse(dr["bMonth"].ToString());
                data.bDay = int.Parse(dr["bDay"].ToString());
                
                string filePath = Path.Combine(profilePath, $"{data.UserId}.png");
                if (System.IO.File.Exists(filePath))
                {
                    data.ProfilePicturePath = $"/images/profile/{data.UserId}.png";
                }
                else
                {
                    data.ProfilePicturePath = ConstatsCommon.representFaceImage;
                }

                data.PersonPraying = ConvertToString(dr, "PersonPraying");
                data.PrayerRecipient = ConvertToString(dr, "PrayerRecipient");
                data.PrayCount = ConvertToInt(dr, "PrayCount");
                data.WhenPray = ConvertToDate(dr, "WhenPray");
                value.Add(data);
            }
            return value;
        }      
    }
}
