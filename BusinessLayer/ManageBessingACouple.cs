using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace BulletinBoard.BusinessLayer
{
    public class ManageBessingACouple
    {
         
        public List<BessingACoupleDatas> getBessingACouple(string Kind, int Id, string ExposeDate)
        {
            ControlBessingACouple controlBessingACouple = new ControlBessingACouple();
            DataSet ds = controlBessingACouple.fetchBessingACouple(Kind, Id, ExposeDate);
            List<BessingACoupleDatas> value = ds.Tables[0].ToList<BessingACoupleDatas>();
            return value;
        }

        public void CreateBessingACouple(string Kind, BessingACoupleData param)
        {
            ControlBessingACouple controlBessingACouple = new ControlBessingACouple();
            controlBessingACouple.CreateBessingACouple(Kind, param.PageNo, param.PrayTitle, param.PrayText, param.PrayHolyWord);
        }
        public void UpdateBessingACouple(string Kind, BessingACoupleData param)
        {
            ControlBessingACouple controlBessingACouple = new ControlBessingACouple();
            controlBessingACouple.UpdateBessingACouple(Kind, param.Id, param.PageNo, param.PrayTitle, param.PrayText, param.PrayHolyWord);
        }
        public void HitBessingACouple(string Kind, int Id)
        {
            ControlBessingACouple controlBessingACouple = new ControlBessingACouple();
            controlBessingACouple.HitBessingACouple(Kind, Id);
        }
        public void DecreasetHitACouple(string Kind, int Id)
        {
            ControlBessingACouple controlBessingACouple = new ControlBessingACouple();
            controlBessingACouple.DecreasetHitACouple(Kind, Id);
        }

        public int GetMaxId(string Kind) => new ControlBessingACouple().GetMaxId(Kind);
        public int GetNextPageNo(string Kind) => new ControlBessingACouple().GetNextPageNo(Kind);

        /// <summary>검증 칸이 표에 이미 만들어져 있는가 (SQL 실행 전이면 false — 화면에서 검증 기능을 숨긴다)</summary>
        public bool IsVerifyReady(string Kind) => new ControlBessingACouple().HasVerifyColumns(Kind);

        public bool VerifyBessingACouple(string Kind, int Id, string by)
            => new ControlBessingACouple().VerifyBessingACouple(Kind, Id, by);

        public bool UnverifyBessingACouple(string Kind, int Id)
            => new ControlBessingACouple().UnverifyBessingACouple(Kind, Id);

        public void DeleteBessingACouple(string Kind, int Id)
        {
            ControlBessingACouple controlBessingACouple = new ControlBessingACouple();
            controlBessingACouple.DeleteBessingACouple(Kind, Id);
        }
    }
}
