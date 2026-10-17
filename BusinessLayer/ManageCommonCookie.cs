using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace BulletinBoard.BusinessLayer
{
    public class ManageCommonCookie
    {
        public List<UserSettingsCookie> getUserCookieSettings(string UserId, string cookieName)
        {
            ControlCommonCookie controlCommonCookie = new ControlCommonCookie();
            DataSet ds = controlCommonCookie.fetchUserCookieSettings(UserId, cookieName);
            List<UserSettingsCookie> value = ds.Tables[0].ToList<UserSettingsCookie>();
            return value;
        }

        public void UpsertUserCookieSettings(string UserId, string cookieName, string cookieValue, int cookieDays)
        {
            ControlCommonCookie controlCommonCookie = new ControlCommonCookie();
            controlCommonCookie.UpsertUserCookieSettings(UserId, cookieName, cookieValue, cookieDays);
        } 
    }
}
