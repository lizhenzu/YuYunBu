using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace BulletinBoard.BusinessLayer
{
    public class ManageApplyEvent
    {
        public List<EventMater> getEventInfo(int EventId)
        {
            ControlApplyEvent controlApplyEvent = new ControlApplyEvent();
            DataSet ds = controlApplyEvent.fetchEventInfo(EventId);
            List<EventMater> value = ds.Tables[0].ToList<EventMater>();
            return value;
        }
        public List<EventMater> getEventAppyInfo(string UserId, int EventId)
        {
            ControlApplyEvent controlApplyEvent = new ControlApplyEvent();
            DataSet ds = controlApplyEvent.fetchEventAppyInfo(UserId, EventId);
            List<EventMater> value = ds.Tables[0].ToList<EventMater>();
            return value;
        }
        public void EnrollEventApplyInfo(string UserId, int EventId)
        {
            ControlApplyEvent controlApplyEvent = new ControlApplyEvent();
            controlApplyEvent.EnrollEventApplyInfo(UserId, EventId);
        }

        public void CancelEnrollEvent(string UserId, int EventId)
        {
            ControlApplyEvent controlApplyEvent = new ControlApplyEvent();
            controlApplyEvent.CancelEnrollEvent(UserId, EventId);
        }
    }
}
