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
    public class ManagePrayFriends
    {
        public List<PrayFriend> getPrayFriend(IWebHostEnvironment _hostingEnvironment)
        {
            ControlPrayFriends controlPrayFriends = new ControlPrayFriends();
            DataSet ds = controlPrayFriends.fetchPrayFreind();
            //     List<UserInfos> value = ds.Tables[0].ToList<UserInfos>();

            List<PrayFriend> value = new List<PrayFriend>();

            var profilePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", "profile");

            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DataRow dr = ds.Tables[0].Rows[i];

                PrayFriend prayFriend = new PrayFriend();

                prayFriend.sequenceNo = long.Parse(dr["sequenceNo"].ToString());

                prayFriend.UserId = dr["UserId"].ToString() as string;
                prayFriend.UserId = dr["UserId"].ToString() as string;
                prayFriend.Email = dr["Email"].ToString() as string;
                prayFriend.FullName = dr["FullName"].ToString() as string;
                prayFriend.UserName = dr["UserName"].ToString() as string;
                prayFriend.FirstName = dr["FirstName"].ToString() as string;
                prayFriend.LastName = dr["LastName"].ToString() as string;
                prayFriend.PhoneNumber = dr["PhoneNumber"].ToString() as string;
                prayFriend.ClassYear = int.Parse(dr["ClassYear"].ToString());
                prayFriend.ClassBan = int.Parse(dr["ClassBan"].ToString());
                prayFriend.bMonth = int.Parse(dr["bMonth"].ToString());
                prayFriend.bDay = int.Parse(dr["bDay"].ToString());
                prayFriend.PrayId = dr["PrayId"].ToString();
                prayFriend.PrayText = dr["PrayText"].ToString();
                prayFriend.insertDT = DateTime.Parse(dr["InsertDT"].ToString());


                string filePath = Path.Combine(profilePath, $"{prayFriend.UserId}.png");
                if (System.IO.File.Exists(filePath))
                {
                    prayFriend.ProfilePicturePath = $"/images/profile/{prayFriend.UserId}.png";
                }
                else
                {
                    prayFriend.ProfilePicturePath = ConstatsCommon.representFaceImage;
                }

                value.Add(prayFriend);
            }
            return value;
        }

        //CreatePrayFreind(string PrayText, string UserName)
        public void CreatePrayFreind(PrayFreind param)
        {
            ControlPrayFriends controlPrayFriends = new ControlPrayFriends();

            controlPrayFriends.CreatePrayFreind(param.PrayText, param.UserName, param.WriteUserName, param.WriteUserId);
        }

        public void UpdatePrayFreind(int PrayId,  string PrayText)
        {
            ControlPrayFriends controlPrayFriends = new ControlPrayFriends();

            controlPrayFriends.UpdatePrayFreind(PrayId, PrayText);
        }
        public void DeletePrayFreind(int PrayId)
        {
            ControlPrayFriends controlPrayFriends = new ControlPrayFriends();

            controlPrayFriends.DeletePrayFreind(PrayId);
        }

        public void HitPrayFreind()
        {
            ControlPrayFriends controlPrayFriends = new ControlPrayFriends();

            controlPrayFriends.HitPrayFreind();
        }

    }
}
