using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace BulletinBoard.BusinessLayer
{
    public class ManageBessingChild
    {
         
        public List<BessingChildDatas> getBessingChild(int Id, string ExposeDate)
        {
            ControlBessingChild controlBessingChild = new ControlBessingChild();
            DataSet ds = controlBessingChild.fetchBessingChild(Id, ExposeDate);
            List<BessingChildDatas> value = ds.Tables[0].ToList<BessingChildDatas>();
            return value;
        }

        public void CreateBessingChild(BessingChildData param)
        {
            ControlBessingChild controlBessingChild = new ControlBessingChild();
            controlBessingChild.CreateBessingChild(param.FromDate, param.ToDate, param.PrayTitle, param.PrayText, param.PrayText_Eng, param.HolyWord);
        }
        public void UpdateBessingChild(BessingChildData param)
        {
            ControlBessingChild controlBessingChild = new ControlBessingChild();
            controlBessingChild.UpdateBessingChild(param.Id, param.FromDate, param.ToDate, param.PrayTitle, param.PrayText, param.PrayText_Eng, param.HolyWord);
        }
        public void HitBessingChild(int Id)
        {
            ControlBessingChild controlBessingChild = new ControlBessingChild();
            controlBessingChild.HitBessingChild(Id);
        }
        public void DecreasetHitWeeklyWordToRead(int Id)
        {
            ControlBessingChild controlBessingChild = new ControlBessingChild();
            controlBessingChild.DecreasetHitWeeklyWordToRead(Id);
        }

        public void DeleteBessingChild(int Id)
        {
            ControlBessingChild controlBessingChild = new ControlBessingChild();
            controlBessingChild.DeleteBessingChild(Id);
        }
    }
}
