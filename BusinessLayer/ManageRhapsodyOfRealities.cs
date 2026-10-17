using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace BulletinBoard.BusinessLayer
{
    public class ManageRhapsodyOfReality
    {
        public List<EveryBibles> getEveryBible(string ExposeDate)
        {
            ControlRhapsodyOfReality controlRhapsodyOfReality = new ControlRhapsodyOfReality();
            DataSet ds = controlRhapsodyOfReality.fetchEveryBible(ExposeDate);
            List<EveryBibles> value = ds.Tables[0].ToList<EveryBibles>();
            return value;
        }
        public List<RhapsodyOfRealitys> getRhapsodyOfReality1(int Id, string ExposeDate)
        {
            ControlRhapsodyOfReality controlRhapsodyOfReality = new ControlRhapsodyOfReality();
            DataSet ds = controlRhapsodyOfReality.fetchRhapsodyOfReality1(Id, ExposeDate);
            List<RhapsodyOfRealitys> value = ds.Tables[0].ToList<RhapsodyOfRealitys>();
            return value;
        }

        public void CreateRhapsodyOfReality(RhapsodyOfReality param)
        {
            ControlRhapsodyOfReality controlRhapsodyOfReality = new ControlRhapsodyOfReality();
            controlRhapsodyOfReality.CreateRhapsodyOfReality(param);
        }
        public void UpdateRhapsodyOfReality(RhapsodyOfReality param)
        {
            ControlRhapsodyOfReality controlRhapsodyOfReality = new ControlRhapsodyOfReality();
            controlRhapsodyOfReality.UpdateRhapsodyOfReality(param);
        }
        public void HitRhapsodyOfReality(int Id)
        {
            ControlRhapsodyOfReality controlRhapsodyOfReality = new ControlRhapsodyOfReality();
            controlRhapsodyOfReality.HitRhapsodyOfReality(Id);
        }

        public void DeleteRhapsodyOfReality(int Id)
        {
            ControlRhapsodyOfReality controlRhapsodyOfReality = new ControlRhapsodyOfReality();
            controlRhapsodyOfReality.DeleteRhapsodyOfReality(Id);
        }
    }
}
