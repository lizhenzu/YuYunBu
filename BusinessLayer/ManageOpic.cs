using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;
using System.Globalization;

namespace BulletinBoard.BusinessLayer
{
    public class ManageOpic
    {
         
        public List<OpicDatas> getOpic(int Id, string ExposeDate, string sortBy, string query)
        {
            ControlOpic controlOpic = new ControlOpic();
            DataSet ds = controlOpic.fetchOpic(Id, ExposeDate, sortBy, query);
            List<OpicDatas> value = ds.Tables[0].ToList<OpicDatas>();
            return value;
        }

        public void CreateOpic(OpicData param)
        {
            ControlOpic controlOpic = new ControlOpic();
            controlOpic.CreateOpic(param.PageNo, param.Question, param.KeyWord1, param.KeyWord2, param.KeyWord3, param.KeyWord4, param.KeyWord1Desc, param.KeyWord2Desc, param.KeyWord3Desc, param.KeyWord4Desc, param.Memo, param.Vocabulary, param.BodyContents);
        }
        public void UpdateOpic(OpicData param)
        {
            ControlOpic controlOpic = new ControlOpic();
            controlOpic.UpdateOpic(param.Id, param.PageNo, param.Question, param.KeyWord1, param.KeyWord2, param.KeyWord3, param.KeyWord4,param.KeyWord1Desc, param.KeyWord2Desc, param.KeyWord3Desc, param.KeyWord4Desc, param.Memo, param.Vocabulary, param.BodyContents);
        }
        public void VisitedOpic(int Id)
        {
            ControlOpic controlOpic = new ControlOpic();
            controlOpic.VisitedOpic(Id);
        }
        public void WorkVisitedOpic(int Id)
        {
            ControlOpic controlOpic = new ControlOpic();
            controlOpic.WorkVisitedOpic(Id);
        }
        public void DeleteOpic(int Id)
        {
            ControlOpic controlOpic = new ControlOpic();
            controlOpic.DeleteOpic(Id);
        }
    }
}
