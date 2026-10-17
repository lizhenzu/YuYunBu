using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace BulletinBoard.BusinessLayer
{
    public class ManagePhotoContest
    {        

        public List<PhotoContests> getPhotoContest(int Id)
        {
            ControlPhotoContest cntrolPhotoContest = new ControlPhotoContest();
            DataSet ds = cntrolPhotoContest.fetchPhotoContest(Id);
            List<PhotoContests> value = ds.Tables[0].ToList<PhotoContests>();
            return value;
        }

      

        public void UpdateJuManNa(JuManNa param)
        {
            ControlJuManNa controlJuManNa = new ControlJuManNa();

            controlJuManNa.UpdaetJuManNa(param);
        }


        public void HitJuManNa(int Id)
        {
            ControlJuManNa controlJuManNa = new ControlJuManNa();
            controlJuManNa.HitJuManNa(Id);
        }
    }
}
