using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace BulletinBoard.BusinessLayer
{
    public class ManageAIPromptWord
    {
        public List<AIPromptWords> getAIPromptWord(int Id)
        {
            ControlAIPromptWord controlAIPromptWord = new ControlAIPromptWord();
            DataSet ds = controlAIPromptWord.fetchAIPromptWord(Id);
            List<AIPromptWords> value = ds.Tables[0].ToList<AIPromptWords>();
            return value;
        }
        public void CreateAIPromptWord(AIPromptWords param)
        {
            ControlAIPromptWord controlAIPromptWord = new ControlAIPromptWord();
            controlAIPromptWord.CreateAIPromptWord(param.PromptText);
        }
        public void UpdateAIPromptWord(AIPromptWords param)
        {
            ControlAIPromptWord controlAIPromptWord = new ControlAIPromptWord();
            controlAIPromptWord.UpdateAIPromptWord(param.Id, param.PromptText);
        }


        public void DeleteAIPromptWord(int Id)
        {
            ControlAIPromptWord controlAIPromptWord = new ControlAIPromptWord();
            controlAIPromptWord.DeleteAIPromptWord(Id);
        }

    }
}
