using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace BulletinBoard.BusinessLayer
{
    public class ManageHelpDesk
    {
        public List<RepairHomePageDatas> getRepairHomePage(int Id)
        {
            ControlHelpDesk controlHelpDesk = new ControlHelpDesk();
            DataSet ds = controlHelpDesk.fetchRepairHomePage(Id);
            List<RepairHomePageDatas> value = ds.Tables[0].ToList<RepairHomePageDatas>();
            return value;
        }
        public void CreateRepairHomePage(RepairHomePageData param)
        {
            ControlHelpDesk controlHelpDesk = new ControlHelpDesk();
            controlHelpDesk.CreateRepairHomePage(param.RequestContents, param.UserName);
        }
        public void DeleteRepairHomePage(int Id)
        {
            ControlHelpDesk controlHelpDesk = new ControlHelpDesk();
            controlHelpDesk.DeleteRepairHomePage(Id);
        }
        public void UpdateRepairHomePage(RepairHomePageData param)
        {
            ControlHelpDesk controlHelpDesk = new ControlHelpDesk();
            controlHelpDesk.UpdateRepairHomePage(param.Id, param.AnswerContents);
        }

    }
}
