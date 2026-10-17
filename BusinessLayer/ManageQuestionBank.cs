using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace BulletinBoard.BusinessLayer
{
    public class ManageQuestionBank
    {
        public List<QuestionBankData> getQuestionBankOrg(int Id, string queryCode)
        {
            ControlQuestionBank controlQuestionBank = new ControlQuestionBank();
            DataSet ds = controlQuestionBank.fetchQuestionBankOrg(Id, queryCode );
            List<QuestionBankData> value = ds.Tables[0].ToList<QuestionBankData>();
            return value;
        }
        public List<QuestionTypeDataObjects> getQuestionBank(int Id, string ExposeDate)
        {
            ControlQuestionBank controlQuestionBank = new ControlQuestionBank();
            DataSet ds = controlQuestionBank.fetchQuestionBank(Id, ExposeDate);
            List<QuestionTypeDataObjects> value = ds.Tables[0].ToList<QuestionTypeDataObjects>();
            return value;
        }

        public void CreateQuizById(int Id, string WhenDate)
        {
            ControlQuestionBank controlQuestionBank = new ControlQuestionBank();
            controlQuestionBank.CreateQuizById(Id, WhenDate);
        }

        public void VerifyQuestionBank(int Id)
        {
            ControlQuestionBank controlQuestionBank = new ControlQuestionBank();
            controlQuestionBank.VerifyQuestionBank(Id);
        }
        public void MinifyQuestionBank(int Id)
        {
            ControlQuestionBank controlQuestionBank = new ControlQuestionBank();
            controlQuestionBank.MinifyQuestionBank(Id);
        }

        public void UpdatePrevRecordUpdateDT(int Id)
        {
            ControlQuestionBank controlQuestionBank = new ControlQuestionBank();
            controlQuestionBank.UpdatePrevRecordUpdateDT(Id);
        }


        public void UpdateNextRecordUpdateDT(int Id)
        {
            ControlQuestionBank controlQuestionBank = new ControlQuestionBank();
            controlQuestionBank.UpdateNextRecordUpdateDT(Id);
        }

        public void DeleteQuestionBank(int Id)
        {
            ControlQuestionBank controlQuestionBank = new ControlQuestionBank();
            controlQuestionBank.DeleteQuestionBank(Id);
        }

        public void UpdateQuestionBank(QuestionBankData question)
        {
            ControlQuestionBank controlQuestionBank = new ControlQuestionBank();
            controlQuestionBank.UpdateQuestionBankData(question.Id, question.QuestionType, question.QuestionQ, question.Question1, question.Question2, question.Question3, question.Question4, question.CorrectAnswer, question.BibleText, question.Jang, question.Zul, question.Zul1);
        }

    }
}
