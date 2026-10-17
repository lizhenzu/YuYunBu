using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;
using System.IO;
using BulletinBoard.Common;
using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml;

namespace BulletinBoard.BusinessLayer
{
    public class ManageReadBible
    {
        public List<Testaments> GetBibleReadingStats(string userId)
        {
            ControleReadBible controleReadBible = new ControleReadBible();
            DataSet ds = controleReadBible.GetBibleReadingStats(userId);
            List<Testaments> value = ds.Tables[0].ToList<Testaments>();
            return value;
        }
        public List<TestamentReadSpeed> GetReadingProgressEstimate(string userId)
        {
            ControleReadBible controleReadBible = new ControleReadBible();
            DataSet ds = controleReadBible.GetReadingProgressEstimate(userId);
            List<TestamentReadSpeed> value = ds.Tables[0].ToList<TestamentReadSpeed>();
            return value;
        }

        public DateTime? GetLastReadDate(string userId)
        {
            ControleReadBible controleReadBible = new ControleReadBible();
            return controleReadBible.GetLastReadDate(userId); 
        }
        public List<TestamentReading> GetUnfinishedBooks(string userId, string BibleText)
        {
            ControleReadBible controleReadBible = new ControleReadBible();
            DataSet ds = controleReadBible.GetUnfinishedBooksWithLastRead(userId, BibleText);
            List<TestamentReading> value = ds.Tables[0].ToList<TestamentReading>();
            return value;
        }
        public decimal GetTotalBibleProgressRate(string userId)
        {
            ControleReadBible controleReadBible = new ControleReadBible();
            return controleReadBible.GetTotalBibleProgressRate(userId);
        }

        public int GetTodayReadVerseCount(string userId)
        {
            ControleReadBible controleReadBible = new ControleReadBible();
            return controleReadBible.FetchTodayReadVerseCount(userId);
        }

        public List<BibleLastReadInfo> GetLastReadVerse(string userId, int from, int to)
        {
            ControleReadBible controleReadBible = new ControleReadBible();
            DataSet ds = controleReadBible.FetchLastReadVerse(userId,from,to);
            List<BibleLastReadInfo> value = ds.Tables[0].ToList<BibleLastReadInfo>();
            return value;
        }
        public List<BibleLastReadRateByUserSimple> GetUserTotalProgressRatesByUser(IWebHostEnvironment _hostingEnvironment, BilbeProcessOption param)
        {
            ControleReadBible controleReadBible = new ControleReadBible();
            DataSet ds = controleReadBible.GetUserTotalProgressRatesByUser(param);
            List<BibleLastReadRateByUserSimple> value = MakeNewDataSetSimple(_hostingEnvironment, ds );
            return value;
        }

        public List<BibleLastReadRateByUserCompleted> GetUserTotalProgressRatesByUserCompleted(IWebHostEnvironment _hostingEnvironment)
        {
            ControleReadBible controleReadBible = new ControleReadBible();
            DataSet ds = controleReadBible.GetUserTotalProgressRatesByUserCompleted();
            List<BibleLastReadRateByUserCompleted> value = MakeNewDataSetSimpleCompleted(_hostingEnvironment, ds);
            return value;
        } 
        private static List<BibleLastReadRateByUsers> MakeNewDataSet(IWebHostEnvironment _hostingEnvironment, DataSet ds)
        {
            List<BibleLastReadRateByUsers> value = new List<BibleLastReadRateByUsers>();

            var profilePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", "profile");

            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DataRow dr = ds.Tables[0].Rows[i];

                BibleLastReadRateByUsers data = new BibleLastReadRateByUsers();
                data.sequenceNo = long.Parse(dr["sequenceNo"].ToString());

                data.UserName = dr["UserName"].ToString() as string;
                data.LastName = dr["LastName"].ToString() as string;
                data.FirstName = dr["FirstName"].ToString() as string;
                data.FullName = dr["FullName"].ToString() as string;

                data.RoleCode = dr["RoleCode"].ToString() as string;
                data.RoleNM = dr["RoleNM"].ToString() as string;
                data.DeptCode = dr["DeptCode"].ToString() as string;
                data.DeptNM = dr["DeptNM"].ToString() as string;

                if (dr["readBibleAllCount"] == DBNull.Value)
                    continue;

                data.readBibleAllCount = int.Parse(dr["readBibleAllCount"].ToString());
                data.ClassYear = int.Parse(dr["ClassYear"].ToString());
                data.ClassBan = int.Parse(dr["ClassBan"].ToString());


                data.UserId = dr["UserId"].ToString() as string;


                string filePath = Path.Combine(profilePath, $"{data.UserId}.png");
                if (System.IO.File.Exists(filePath))
                {
                    data.ProfilePicturePath = $"/images/profile/{data.UserId}.png";
                }
                else
                {
                    data.ProfilePicturePath = ConstatsCommon.representFaceImage;
                }

                data.TotalProgressRate = Decimal.Parse(dr["TotalProgressRate"].ToString());
                data.ReadDate = DateTime.Parse(dr["ReadDate"].ToString());

                value.Add(data);
            }

            return value;
        }

        private static List<BibleLastReadRateByUserSimple> MakeNewDataSetSimple(IWebHostEnvironment _hostingEnvironment, DataSet ds)
        {
            List<BibleLastReadRateByUserSimple> value = new List<BibleLastReadRateByUserSimple>();

            var profilePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", "profile");

            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DataRow dr = ds.Tables[0].Rows[i];

                BibleLastReadRateByUserSimple data = new BibleLastReadRateByUserSimple();
                data.sequenceNo = long.Parse(dr["sequenceNo"].ToString());

                data.UserName = dr["UserName"].ToString() as string;
                data.LastName = dr["LastName"].ToString() as string;
                data.FirstName = dr["FirstName"].ToString() as string;
                data.UserId = dr["UserId"].ToString() as string;

                string filePath = Path.Combine(profilePath, $"{data.UserId}.png");
                if (System.IO.File.Exists(filePath))
                {
                    data.ProfilePicturePath = $"/images/profile/{data.UserId}.png";
                }
                else
                {
                    data.ProfilePicturePath = ConstatsCommon.representFaceImage;
                }
                data.TotalProgressRate = Decimal.Parse(dr["TotalProgressRate"].ToString());
                data.ReadDate = DateTime.Parse(dr["ReadDate"].ToString());
                data.readBibleAllCount = int.Parse(dr["readBibleAllCount"].ToString());
                data.RoleCode  = dr["RoleCode"].ToString();
                data.RoleNM    = dr["RoleNM"].ToString();
                data.ClassYear = int.TryParse(dr["ClassYear"].ToString(), out int cy) ? cy : 0;
                data.ClassBan  = int.TryParse(dr["ClassBan"].ToString(),  out int cb) ? cb : 0;

                value.Add(data);
            }

            return value;
        }

        private static List<BibleLastReadRateByUserCompleted> MakeNewDataSetSimpleCompleted(IWebHostEnvironment _hostingEnvironment, DataSet ds)
        {
            List<BibleLastReadRateByUserCompleted> value = new List<BibleLastReadRateByUserCompleted>();

            var profilePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", "profile");

            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DataRow dr = ds.Tables[0].Rows[i];

                BibleLastReadRateByUserCompleted data = new BibleLastReadRateByUserCompleted();
                data.sequenceNo = long.Parse(dr["sequenceNo"].ToString());

                data.UserName = dr["UserName"].ToString() as string;
                data.LastName = dr["LastName"].ToString() as string;
                data.FirstName = dr["FirstName"].ToString() as string;
                data.UserId = dr["UserId"].ToString() as string;
                data.readBibleAllCount = int.Parse(dr["readBibleAllCount"].ToString());

                

                string filePath = Path.Combine(profilePath, $"{data.UserId}.png");
                if (System.IO.File.Exists(filePath))
                {
                    data.ProfilePicturePath = $"/images/profile/{data.UserId}.png";
                }
                else
                {
                    data.ProfilePicturePath = ConstatsCommon.representFaceImage;
                }
              
                value.Add(data);
            }

            return value;
        }


    }
}
