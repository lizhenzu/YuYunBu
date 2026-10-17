using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;
using BulletinBoard.Models.AccountViewModels;

namespace BulletinBoard.BusinessLayer
{
    public class ManageAccount
    {     
        public void UpdateAspNetUsers(RegisterViewModel registerViewModel)
        {
            ControlAccount controlAccount = new ControlAccount();

            controlAccount.UpdateAspNetUsers(registerViewModel.Email, registerViewModel.UserNameKOR, registerViewModel.roleST);
        }

        public string GetAspNetUserRoles(string UserName)
        {
            ControlAccount controlAccount = new ControlAccount();

            DataSet ds = controlAccount.GetAspNetUserRoles(UserName);

            return ds.Tables[0].Rows[0][0].ToString(); 
        }
    }
}
