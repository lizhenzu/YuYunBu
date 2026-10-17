using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace BulletinBoard.BusinessLayer
{
    public class ManagePraiseContents
    {
        public List<PraiseContents> getPraiseContents(string kind,int Id)
        {
            ControlPraiseContents controlPraiseContents = new ControlPraiseContents();
            DataSet ds = controlPraiseContents.fetchPraiseContents(kind,Id);
            List<PraiseContents> value = ds.Tables[0].ToList<PraiseContents>();
            return value;
        }

        public void CreatePraiseContents(PraiseContent param)
        {
            ControlPraiseContents controlPraiseContents = new ControlPraiseContents();
            controlPraiseContents.CreatePraiseContents(param.contentsURL, param.contentsText, param.Kind, param.Singer); 
        }

        public void UpdatePraiseContents(PraiseContent param)
        {
            ControlPraiseContents controlPraiseContents = new ControlPraiseContents();
            controlPraiseContents.UpdatePraiseContents(param.Id, param.contentsURL, param.contentsText, param.Kind, param.Singer);
        }
    
        public void DeletePraiseContents(int Id)
        {
            ControlPraiseContents controlPraiseContents = new ControlPraiseContents();
            controlPraiseContents.deletePraiseContents(Id);
        }
    }
}
