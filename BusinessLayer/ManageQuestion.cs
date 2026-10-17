using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace BulletinBoard.BusinessLayer
{
    public class ManageQuestion
    {
        
        public List<QuestionTypeDataObjects> getQuestionObject(int QuestionDataId, bool activeData, string questionType, bool queryByMonth, bool queryMoreThan , DateTime workDT, string KindData, string UserName = "", bool AddPoint = false)
        {
            ControlQuestion controlQuestion = new ControlQuestion();
            DataSet ds = controlQuestion.fetchQuestion(QuestionDataId, activeData, questionType, queryByMonth, queryMoreThan, workDT, KindData, UserName, AddPoint);
            List<QuestionTypeDataObjects> value = ds.Tables[0].ToList<QuestionTypeDataObjects>();
            return value;
        }

        public List<QuestionTypeDatas> getQuestionSubject(int QuestionDataId, bool activeData, string questionType, bool queryByMonth, bool queryMoreThan, DateTime workDT, string KindData)
        {
            ControlQuestion controlQuestion = new ControlQuestion();
            DataSet ds = controlQuestion.fetchQuestion(QuestionDataId, activeData, questionType, queryByMonth, queryMoreThan, workDT, KindData);
            List<QuestionTypeDatas> value = ds.Tables[0].ToList<QuestionTypeDatas>();
            return value;
        }
        public void CreateQuestionObject(QuestionTypeDataObject question)
        {
            ControlQuestion controlQuestion = new ControlQuestion();
            controlQuestion.CreateQuestionObject(question.ExposeDate, question.QuestionType, question.QuestionBackground, question.QuestionQ, question.Question1, question.Question2, question.Question3, question.Question4, question.AnswerQ, question.CorrectAnswer, question.AnswerGuide, question.UserName);
        }

        public void CreateQuestionSubject(QuestionTypeData question)
        {
            ControlQuestion controlQuestion = new ControlQuestion();
            controlQuestion.CreateQuestionSubject(question.ExposeDate, question.QuestionType, question.QuestionBackground, question.QuestionQ, question.AnswerQ, question.CorrectAnswer, question.AnswerGuide, question.UserName);
        }

        public void UpdateQuestionObject(QuestionTypeDataObject question)
        {
            ControlQuestion controlQuestion = new ControlQuestion();
            controlQuestion.UpdateQuestionObject(question.Id, question.ExposeDate, question.QuestionType, question.QuestionBackground, question.QuestionQ, question.Question1, question.Question2, question.Question3, question.Question4, question.AnswerQ, question.CorrectAnswer, question.AnswerGuide, question.appendedComments);
        }

        public void UpdateQuestionSubject(QuestionTypeData question)
        {
            ControlQuestion controlQuestion = new ControlQuestion();
            controlQuestion.UpdateQuestionSubject(question.Id, question.ExposeDate, question.QuestionType, question.QuestionBackground, question.QuestionQ, question.AnswerQ, question.CorrectAnswer, question.AnswerGuide, question.appendedComments );
        }

        public void DeleteQuestion(int Id)
        {
            ControlQuestion controlQuestion = new ControlQuestion();
            controlQuestion.DeleteQuestion(Id);
        }

    }
}
