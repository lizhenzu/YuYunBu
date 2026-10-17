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
    public class ManageCheckPoint
    {
        public List<AwardItemByUser> getAwardItemPickAll(string UserId)
        {
            ControlCheckPoint controlCheckPoint = new ControlCheckPoint();
            DataSet ds = controlCheckPoint.fetchAwardItemPickAll(UserId);

            List<AwardItemByUser> value = ds.Tables[0].ToList<AwardItemByUser>();
            return value;
        }
        public List<AwardItemPick> getAwardItemPick(string UserId)
        {
            ControlCheckPoint controlCheckPoint = new ControlCheckPoint();
            DataSet ds = controlCheckPoint.fetchAwardItemPick(UserId);

            List<AwardItemPick> value = ds.Tables[0].ToList<AwardItemPick>();
            return value;
        }


        public List<PointByUserName> getGetThisWeekRewarding(string DeptCode, IWebHostEnvironment _hostingEnvironment)
        {
            ControlCheckPoint controlCheckPoint = new ControlCheckPoint();
            DataSet ds = controlCheckPoint.fetchGetThisWeekRewarding(DeptCode);

            List<PointByUserName> value = MakeNewDataSet(_hostingEnvironment, ds);

            return value;
        }


        public List<CheckPoint> getViewAllPointInfo(string UserId)
        {
            ControlCheckPoint controlCheckPoint = new ControlCheckPoint();
            DataSet ds = controlCheckPoint.fetchViewAllPointInfo(UserId);

            List<CheckPoint> value = ds.Tables[0].ToList<CheckPoint>();

            return value;
        }

        public List<PointAwardUser> getGetThisWeekRewardingByAward(string DeptCode, IWebHostEnvironment _hostingEnvironment)
        {
            List<PointByUserName> aadd = getGetThisWeekRewarding(DeptCode, _hostingEnvironment);
            List<AwardItemByUser> aa = getAwardItemPickAll(string.Empty);

            // 병합 결과 리스트
            List<PointAwardUser> result = aadd.Select(user => {
                var award = aa.FirstOrDefault(a => a.UserId == user.UserId);

                return new PointAwardUser
                {
                    // PointByUserName 속성 복사
                    sequenceNo = user.sequenceNo,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    DC = user.DC,
                    DC_Total = user.DC_Total,
                    Total = user.Total,
                    ClassYear = user.ClassYear,
                    ClassBan = user.ClassBan,
                    minclickDate = user.minclickDate,
                    maxclickDate = user.maxclickDate,
                    AwardLevel = user.AwardLevel,
                    AwardLevel1000Value = user.AwardLevel1000Value,
                    UserId = user.UserId,
                    ProfilePicturePath = user.ProfilePicturePath,
                    RoleNM = user.RoleNM,
                    DeptNM = user.DeptNM,
                    RoleCode = user.RoleCode,
                    DeptCode = user.DeptCode,

                    // AwardItemByUser 속성 복사 (nullable로 설정)
                    ProductID = award?.ProductID,
                    ProductName = award?.ProductName,
                    ProductDescription = award?.ProductDescription,
                    UnitPrice = award?.UnitPrice,
                    PointRequirement = award?.PointRequirement
                };
            }).ToList();

            return result;
        }



        public List<PointByUserName> getGroupByUsernameData(RankQueryOption rankQueryOption, string UserId, string orderBy, string DeptCode, IWebHostEnvironment hostingEnv)
        {
            ControlCheckPoint controlCheckPoint = new ControlCheckPoint();
            DataSet ds = controlCheckPoint.fetchGroupByUsernameDataN(rankQueryOption, UserId, DeptCode, orderBy);            

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return new List<PointByUserName>();

            // 2) DataTable -> List<ExpectedAward> (사용 중인 확장 메서드 가정)
            List<PointByUserName> list = ds.Tables[0].ToList<PointByUserName>() ?? new List<PointByUserName>();


            // 3) 프로필 이미지 기본 경로 준비 (WebRootPath null 대비)
            string webRoot = hostingEnv?.WebRootPath ?? string.Empty;
            string profilePath = Path.Combine(webRoot, "images", "profile");

            // 4) 각 사용자별 프로필 이미지 존재 확인 후 경로 설정
            foreach (var ea in list)
            {
                // UserId가 null일 수 있다면 대비 (ExpectedAward.UserId 타입에 맞춰 조정)
                var userIdStr = ea.UserId?.ToString();
                if (string.IsNullOrWhiteSpace(userIdStr))
                {
                    ea.ProfilePicturePath = ConstatsCommon.representFaceImage; // 프로젝트 상수 사용(기존 명칭 유지)
                    continue;
                }

                // png 고정 사용
                string filePath = Path.Combine(profilePath, userIdStr + ".png");
                if (!string.IsNullOrEmpty(webRoot) && File.Exists(filePath))
                {
                    ea.ProfilePicturePath = $"/images/profile/{userIdStr}.png";
                }
                else
                {
                    ea.ProfilePicturePath = ConstatsCommon.representFaceImage;
                }
            }

            return list; 
        }


		public List<PointByUserName> getGroupByUsernameDataByClass(int ClassYear, int ClassBan, string orderBy, IWebHostEnvironment _hostingEnvironment)
		{
			ControlCheckPoint controlCheckPoint = new ControlCheckPoint();
			DataSet ds = controlCheckPoint.fetchGroupByUsernameDataByClass(ClassYear, ClassBan, orderBy);

			List<PointByUserName> value = MakeNewDataSet(_hostingEnvironment, ds);

			return value;
		}

		private static List<PointByUserName> MakeNewDataSet(IWebHostEnvironment _hostingEnvironment, DataSet ds)
        {
            List<PointByUserName> value = new List<PointByUserName>();

            var profilePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", "profile");

            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DataRow dr = ds.Tables[0].Rows[i];

                PointByUserName pointByUserName = new PointByUserName();
                pointByUserName.sequenceNo = long.Parse(dr["sequenceNo"].ToString());
                pointByUserName.UserName = dr["UserName"].ToString() as string;
                pointByUserName.FirstName = dr["FirstName"].ToString() as string;
                pointByUserName.LastName = dr["LastName"].ToString() as string;
                pointByUserName.FullName = dr["FullName"].ToString() as string;

                pointByUserName.RoleNM = dr["RoleNM"].ToString() as string;
                pointByUserName.DeptNM = dr["DeptNM"].ToString() as string;
                pointByUserName.RoleCode = dr["RoleCode"].ToString() as string;
                pointByUserName.DeptCode = dr["DeptCode"].ToString() as string;
                 

                pointByUserName.DC = int.Parse(dr["DC"].ToString());
                pointByUserName.DC_Total = int.Parse(dr["DC_Total"].ToString());
                pointByUserName.Total = int.Parse(dr["Total"].ToString());
                pointByUserName.ClassYear = int.Parse(dr["ClassYear"].ToString());
                pointByUserName.ClassBan = int.Parse(dr["ClassBan"].ToString());

                pointByUserName.AwardLevel = int.Parse(dr["AwardLevel"].ToString());
                pointByUserName.AwardLevel1000Value = int.Parse(dr["AwardLevel1000Value"].ToString());
                pointByUserName.UserId = dr["UserId"].ToString() as string;

                pointByUserName.minclickDate = DateTime.Parse(dr["minclickDate"].ToString());
                pointByUserName.maxclickDate = DateTime.Parse(dr["maxclickDate"].ToString());

                string filePath = Path.Combine(profilePath, $"{pointByUserName.UserId}.png");
                if (System.IO.File.Exists(filePath))
                {
                    pointByUserName.ProfilePicturePath = $"/images/profile/{pointByUserName.UserId}.png";
                }
                else
                {
                    pointByUserName.ProfilePicturePath = ConstatsCommon.representFaceImage;
                }

                value.Add(pointByUserName);
            }

            return value;
        }

        public List<PointByClass> getGroupByClassData(RankQueryOption rankQueryOption, int duration, string DeptCode)
        {
            ControlCheckPoint controlCheckPoint = new ControlCheckPoint();
            DataSet ds = controlCheckPoint.fetchGroupByClassData(rankQueryOption, duration, DeptCode);
            List<PointByClass> value = ds.Tables[0].ToList<PointByClass>();
            return value;
        }


        //public List<ExpectedAward> getExpectedAward(IHostingEnvironment _hostingEnvironment)
        //{
        //    ControlCheckPoint controlCheckPoint = new ControlCheckPoint();
        //    DataSet ds = controlCheckPoint.fetchExpectedAward();
        //    List<ExpectedAward> value = ds.Tables[0].ToList<ExpectedAward>();

        //    var profilePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", "profile");

        //    for(DataRow va in value)
        //    {
        //        string filePath = Path.Combine(profilePath, $"{va.UserId}.png");
        //        if (System.IO.File.Exists(filePath))
        //        {
        //            va.ProfilePicturePath = $"/images/profile/{va.UserId}.png";
        //        }
        //        else
        //        {
        //            va.ProfilePicturePath = ConstatsCommon.representFaceImage;
        //        }
        //    }

        //    return value;
        //}



        public List<AwardBase> getAwardBase()
        {
            ControlCheckPoint controlCheckPoint = new ControlCheckPoint();
            DataSet ds = controlCheckPoint.fetchAwardBase();
            List<AwardBase> value = ds.Tables[0].ToList<AwardBase>();
            return value;
        }

        public List<ExpectedAward> getExpectedAward(IWebHostEnvironment hostingEnv)
        {
            // 1) 데이터셋 조회 및 안전성 검사
            var controlCheckPoint = new ControlCheckPoint();
            DataSet ds = controlCheckPoint.fetchExpectedAward();

            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return new List<ExpectedAward>();

            // 2) DataTable -> List<ExpectedAward> (사용 중인 확장 메서드 가정)
            List<ExpectedAward> list = ds.Tables[0].ToList<ExpectedAward>() ?? new List<ExpectedAward>();

            // 3) 프로필 이미지 기본 경로 준비 (WebRootPath null 대비)
            string webRoot = hostingEnv?.WebRootPath ?? string.Empty;
            string profilePath = Path.Combine(webRoot, "images", "profile");

            // 4) 각 사용자별 프로필 이미지 존재 확인 후 경로 설정
            foreach (var ea in list)
            {
                // UserId가 null일 수 있다면 대비 (ExpectedAward.UserId 타입에 맞춰 조정)
                var userIdStr = ea.UserId?.ToString();
                if (string.IsNullOrWhiteSpace(userIdStr))
                {
                    ea.ProfilePicturePath = ConstatsCommon.representFaceImage; // 프로젝트 상수 사용(기존 명칭 유지)
                    continue;
                }

                // png 고정 사용
                string filePath = Path.Combine(profilePath, userIdStr + ".png");
                if (!string.IsNullOrEmpty(webRoot) && File.Exists(filePath))
                {
                    ea.ProfilePicturePath = $"/images/profile/{userIdStr}.png";
                }
                else
                {
                    ea.ProfilePicturePath = ConstatsCommon.representFaceImage;
                }
            }

            return list;
        }

        public void UpOrDownAwardData(string userName, string optionData, int value)
        {
            ControlCheckPoint controlCheckPoint = new ControlCheckPoint();
            controlCheckPoint.UpOrDownAwardData(userName, optionData, value);  
        }

        public void SetAwardData(string userName, int value)
        {
            ControlCheckPoint controlCheckPoint = new ControlCheckPoint();
            controlCheckPoint.SetAwardData(userName, value);
        }

        public void UpsertAwardItemPick(string userId, int productId)
        {
            ControlCheckPoint controlCheckPoint = new ControlCheckPoint();
            controlCheckPoint.UpsertAwardItemPick(userId, productId);
        }
        public void UpdateAwardItem(AwardItem item)
        {
            ControlCheckPoint controlCheckPoint = new ControlCheckPoint();
            controlCheckPoint.UpdateAwardItem(item);
        }
        public void DeleteAwardItemPick(string UserId)
        {
            ControlCheckPoint controlCheckPoint = new ControlCheckPoint();
            controlCheckPoint.DeleteAwardItemPick(UserId);
        }
    }//
}
