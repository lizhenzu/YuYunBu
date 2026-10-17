using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace BulletinBoard.BusinessLayer
{
    public class ManageAIExperience
    {         
        public List<AIAskHistorys> getAIAskHistory(int Id)
        {
            ControlAIExperience controlAIExperience = new ControlAIExperience();
            DataSet ds = controlAIExperience.fetchAIAskHistory(Id);
            List<AIAskHistorys> value = ds.Tables[0].ToList<AIAskHistorys>();
            return value;
        }  
    }
}
