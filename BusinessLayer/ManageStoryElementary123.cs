using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace BulletinBoard.BusinessLayer
{
    public class ManageStoryElementary123
    {
        public List<StoryElementary123s> getStoryElementary123(string kind,int Id)
        {
            ControlStoryElementary123 controlStoryElementary123 = new ControlStoryElementary123();
            DataSet ds = controlStoryElementary123.fetchStoryElementary123(kind,Id);
            List<StoryElementary123s> value = ds.Tables[0].ToList<StoryElementary123s>();
            return value;
        }

        public void CreateStoryElementary123(PraiseContent param)
        {
            ControlStoryElementary123 controlStoryElementary123 = new ControlStoryElementary123();
            controlStoryElementary123.CreateStoryElementary123(param.contentsURL, param.contentsText, param.Kind); 
        }

        public void UpdateStoryElementary123(PraiseContent param)
        {
            ControlStoryElementary123 controlStoryElementary123 = new ControlStoryElementary123();
            controlStoryElementary123.UpdateStoryElementary123(param.Id, param.contentsURL, param.contentsText, param.Kind);
        }
    
        public void DeleteStoryElementary123(int Id)
        {
            ControlStoryElementary123 controlStoryElementary123 = new ControlStoryElementary123();
            controlStoryElementary123.deleteStoryElementary123(Id);
        }
    }
}
