using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.Extensions.Logging;

namespace BulletinBoard.BusinessLayer
{
    public class ManageHumanResource
    {
        public List<UserInfo> getUserInfos(IWebHostEnvironment _hostingEnvironment, string UserId, string UserName, int PersonId, int ClassYear, int ClassBan,  string BanMode, string DeptCode, RankQueryOption RankQueryOptionData)
        {
            ControlHumanResource controlHumanResource = new ControlHumanResource();
            DataSet ds = controlHumanResource.fetchgetUserInfos(UserId, UserName, PersonId, ClassYear, ClassBan, BanMode, DeptCode, RankQueryOptionData);
            //     List<UserInfos> value = ds.Tables[0].ToList<UserInfos>();

            List<UserInfo> value = new List<UserInfo>();

            MakeUserList(_hostingEnvironment, RankQueryOptionData, ds, value);

            return value;
        }

        public List<UserInfo> getApplyEventList(IWebHostEnvironment _hostingEnvironment, int EventId)
        {
            ControlHumanResource controlHumanResource = new ControlHumanResource();
            DataSet ds = controlHumanResource.fetchApplyEventList(EventId);
            //     List<UserInfos> value = ds.Tables[0].ToList<UserInfos>();

            List<UserInfo> value = new List<UserInfo>();

            MakeUserList(_hostingEnvironment, RankQueryOption.None, ds, value);

            return value;
        }

        private static void MakeUserList(IWebHostEnvironment _hostingEnvironment, RankQueryOption RankQueryOptionData, DataSet ds, List<UserInfo> value)
        {
            var profilePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", "profile");

            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DataRow dr = ds.Tables[0].Rows[i];
                // Read the varbinary(max) field as byte array
                // Convert byte array to Base64 string
                //if (profilePictureBytes != null)
                //{
                //    // ProfilePicture = Convert.ToBase64String(profilePictureBytes);
                //    //ProfilePicture = SharedComm.CompressAndConvertToBase64(profilePictureBytes);
                //    ProfilePicture = SharedComm.CompressAndConvertToBase64(profilePictureBytes);
                //}
                UserInfo userInfos = new UserInfo();

                userInfos.sequenceNo = long.Parse(dr["sequenceNo"].ToString());

                userInfos.UserId = dr["UserId"].ToString() as string;
                userInfos.Email = dr["Email"].ToString() as string;
                userInfos.FullName = dr["FullName"].ToString() as string;
                userInfos.UserName = dr["UserName"].ToString() as string;
                userInfos.FirstName = dr["FirstName"].ToString() as string;
                userInfos.LastName = dr["LastName"].ToString() as string;
                userInfos.PhoneNumber = dr["PhoneNumber"].ToString() as string;
                userInfos.PhoneNumber2 = dr["PhoneNumber2"].ToString() as string;
                userInfos.readBibleAllCount = int.Parse(dr["readBibleAllCount"].ToString());
                userInfos.ClassYear = int.Parse(dr["ClassYear"].ToString());
                userInfos.ClassBan = int.Parse(dr["ClassBan"].ToString());

                userInfos.HomeAaddress = dr["HomeAaddress"].ToString() as string;
                userInfos.school = dr["school"].ToString() as string;
                userInfos.sex = dr["sex"].ToString() as string;

                //userInfos.insertDT = DateTime.Parse(dr["insertDT"].ToString());
                
                userInfos.DeptCode = dr["DeptCode"].ToString() as string;
                userInfos.DeptNM = dr["DeptNM"].ToString() as string;
                userInfos.RoleCode = dr["RoleCode"].ToString() as string;
                userInfos.RoleNM = dr["RoleNM"].ToString() as string;

                userInfos.bMonth = int.Parse(dr["bMonth"].ToString());
                userInfos.bDay = int.Parse(dr["bDay"].ToString());
                userInfos.birthDay_PublishYN = dr["birthDay_PublishYN"].ToString();
                userInfos.PersonId = int.Parse(dr["PersonId"].ToString());
                //userInfos.DC_Total = int.Parse(dr["DC_Total"].ToString());
                //userInfos.countD = int.Parse(dr["countD"].ToString());



                string filePath = Path.Combine(profilePath, $"{userInfos.UserId}.png");

                if (string.IsNullOrEmpty(userInfos.LastName))
                {
                    

                    if (!string.IsNullOrWhiteSpace(userInfos.FullName))
                    {
                        var name = userInfos.FullName;

                        userInfos.LastName = name.Length switch
                        {
                            >= 2 => name.Substring(0, 1),
                            1 => name.Substring(0, 1),
                            _ => name
                        };
                    }
                }

                if (string.IsNullOrEmpty(userInfos.FirstName))
                {
                    if (!string.IsNullOrWhiteSpace(userInfos.FullName))
                    {
                        var name = userInfos.FullName;

                        userInfos.FirstName = name.Length switch
                        {
                            >= 3 => name.Substring(1, 2),
                            2 => name.Substring(1, 1),
                            _ => name
                        };
                    }
                }

                if (System.IO.File.Exists(filePath))
                {
                    userInfos.ProfilePicturePath = $"/images/profile/{userInfos.UserId}.png";
                }
                else
                {
                    userInfos.ProfilePicturePath = ConstatsCommon.representFaceImage;
                }

                if (RankQueryOptionData == RankQueryOption.NoPhoto)
                {
                    if (!System.IO.File.Exists(filePath))
                    {
                        value.Add(userInfos);
                    }
                }
                else
                {
                    value.Add(userInfos);
                }



            }
        }

        public List<UserInfo> getClassInfos()
        {
            ControlHumanResource controlHumanResource = new ControlHumanResource();
            DataSet ds = controlHumanResource.fetchgetClassInfos();
            List<UserInfo> value = ds.Tables[0].ToList<UserInfo>();
            return value;
        }

        public void CreatePopMessage(PopMessage param)
        {
            ControlHumanResource controlHumanResource = new ControlHumanResource();
          //  controlHumanResource.CreatePopMessage(param.MessageText, param.FromWhom, param.ToWhom);
        }

        public void UpdatePopMessage(PopMessage param)
        {
            ControlHumanResource controlHumanResource = new ControlHumanResource();
          //  controlHumanResource.UpdatePopMessage(param.Id, param.MessageText, param.FromWhom, param.ToWhom);
        }

        public void DeletePopMessage(int Id)
        {
            ControlHumanResource controlHumanResource = new ControlHumanResource();
         //   controlHumanResource.DeletePopMessage(Id);
        }

        internal void UpdateHitPopMessage(string userID)
        {
            ControlHumanResource controlHumanResource = new ControlHumanResource();
///controlHumanResource.UpdateHitPopMessage(userID);
        }
    }
}
