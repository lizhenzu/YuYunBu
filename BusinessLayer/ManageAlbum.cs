using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace BulletinBoard.BusinessLayer
{
    public class ManageAlbum
    {         
        public List<UserInfoBase2> getMyPhotoPick()
        {
            ControlAlbum controlAlbum = new ControlAlbum();
            DataSet ds = controlAlbum.fetchMyPhotoPick();
            List<UserInfoBase2> value = ds.Tables[0].ToList<UserInfoBase2>();
            return value;
        }  
    }
}
