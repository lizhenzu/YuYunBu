using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace BulletinBoard.BusinessLayer
{
    public class ManageClickHistoryForQuestion
    {

        public List<QuestionProcessStatus> getCurrentStatus(string UserName, DateTime workDT, int Id = 0)
        {
            ControlClickHistoryForQuestion controlClickHistory = new ControlClickHistoryForQuestion();
            DataSet ds = controlClickHistory.fetchCurrentStatus(UserName, workDT, Id);

            List<QuestionProcessStatus> value = ds.Tables[0].ToList<QuestionProcessStatus>();

            return value;
        }

        public int CountClickHistoryForQuestion(string UserName, int QuestionId, bool todayQuery, DateTime workDT)
        {
            ControlClickHistoryForQuestion controlClickHistory = new ControlClickHistoryForQuestion();

            return controlClickHistory.CountClickHistoryForQuestion(UserName, QuestionId, todayQuery, workDT);
        }

        public List<MismatchClickHistoryForQuestions> MismatchClickHistoryForQuestion()
        {
            ControlClickHistoryForQuestion controlClickHistory = new ControlClickHistoryForQuestion();
            DataSet ds = controlClickHistory.MismatchClickHistoryForQuestion();

            List<MismatchClickHistoryForQuestions> value = ds.Tables[0].ToList<MismatchClickHistoryForQuestions>();

            return value;
        }

        public void CorrectMismatchClickHistoryForQuestion(int Id, int QuestionId, string UserName)
        {
            ControlClickHistoryForQuestion controlClickHistory = new ControlClickHistoryForQuestion();
            controlClickHistory.CorrectMismatchClickHistoryForQuestion(Id, QuestionId, UserName);
        }

        public void CreateClickHistoryForQuestion(int  QuestionId, string UserName, string AnswerQ, string CorrectAnswer, DateTime workDT, int cYear)
        {
            ControlClickHistoryForQuestion controlClickHistory = new ControlClickHistoryForQuestion();

            controlClickHistory.CreateClickHistoryForQuestion(QuestionId, UserName, AnswerQ, CorrectAnswer, workDT, cYear);
        }
        public void DeleteClickHistoryForQuestion(int QuestionId, string UserName)
        {
            ControlClickHistoryForQuestion controlClickHistory = new ControlClickHistoryForQuestion();

            controlClickHistory.DeleteClickHistoryForQuestion(QuestionId, UserName);
        }

        
    }
}
