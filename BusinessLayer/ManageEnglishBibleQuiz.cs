using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace BulletinBoard.BusinessLayer
{
    public class ManageEnglishBibleQuiz
    {
        public List<EnglishBibleQuiz> getEnglishBibleQuiz(string UserName, int Id)
        {
            ControlEnglishBibleQuiz controlEnglishBibleQuiz = new ControlEnglishBibleQuiz();
            DataSet ds = controlEnglishBibleQuiz.fetchEnglishBibleQuiz(UserName, Id);
            List<EnglishBibleQuiz> value = ds.Tables[0].ToList<EnglishBibleQuiz>();
            return value;
        }
        public List<EnglishBibleQuiz> getEnglishBibleQuizCount()
        {
            ControlEnglishBibleQuiz controlEnglishBibleQuiz = new ControlEnglishBibleQuiz();
            DataSet ds = controlEnglishBibleQuiz.fetchEnglishBibleQuizCount();
            List<EnglishBibleQuiz> value = ds.Tables[0].ToList<EnglishBibleQuiz>();
            return value;
        }

        public int getEnglishBibleQuizSuccessCount(string UserName, bool SuccessOrNot, int Id)
        {
            ControlEnglishBibleQuiz controlEnglishBibleQuiz = new ControlEnglishBibleQuiz();
            return controlEnglishBibleQuiz.fetchEnglishBibleQuizSuccessCount(UserName, SuccessOrNot, Id);
        }

        public int getClickHistoryForEnglishBibleQuiz(string UserName, DateTime workDT)
        {
            ControlEnglishBibleQuiz controlEnglishBibleQuiz = new ControlEnglishBibleQuiz();
            DataSet ds = controlEnglishBibleQuiz.fetchTodayClickHistoryForEnglishBibleQuiz(UserName, workDT);

            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                string value = ds.Tables[0].Rows[0][0].ToString();
                return int.Parse(value);
            }
            return 0;
        }

        public List<EnglishBibleQuizRank> getEnglishBibleQuizRank(IWebHostEnvironment _hostingEnvironment)
        {
            ControlEnglishBibleQuiz controlEnglishBibleQuiz = new ControlEnglishBibleQuiz();
            DataSet ds = controlEnglishBibleQuiz.fetchEnglishBibleQuizRank();

            List<EnglishBibleQuizRank> value = new List<EnglishBibleQuizRank>();

            var profilePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", "profile");

            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                DataRow dr = ds.Tables[0].Rows[i];


                EnglishBibleQuizRank eglishBibleQuizRank = new EnglishBibleQuizRank();
                eglishBibleQuizRank.sequenceNo = long.Parse(dr["sequenceNo"].ToString());
                eglishBibleQuizRank.userName = dr["userName"].ToString() as string;
                eglishBibleQuizRank.HideName = dr["HideName"].ToString() as string;
                eglishBibleQuizRank.Id = dr["Id"].ToString() as string;
                eglishBibleQuizRank.DC = int.Parse(dr["DC"].ToString());
                eglishBibleQuizRank.DC_Success = int.Parse(dr["DC_Success"].ToString());
                eglishBibleQuizRank.DC_Fail = int.Parse(dr["DC_Fail"].ToString());

                string filePath = Path.Combine(profilePath, $"{eglishBibleQuizRank.Id}.png");

                if (System.IO.File.Exists(filePath))
                {
                    eglishBibleQuizRank.ProfilePicturePath = $"/images/profile/{eglishBibleQuizRank.Id}.png";
                }
                else
                {
                    eglishBibleQuizRank.ProfilePicturePath = ConstatsCommon.representFaceImage;
                }
                value.Add(eglishBibleQuizRank);
            }

            return value;
        }

        public void CreateClickHistoryForEnglishBibleQuiz(string UserName, int QuestionId, string AnswerQ)
        {
            ControlEnglishBibleQuiz controlEnglishBibleQuiz = new ControlEnglishBibleQuiz();
            controlEnglishBibleQuiz.CreateClickHistoryForEnglishBibleQuiz(UserName, QuestionId, AnswerQ);
        }
    }
}
