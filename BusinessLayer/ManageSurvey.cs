using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace BulletinBoard.BusinessLayer
{
    public class ManageSurvey
    {
        public List<SurveyDatas> getSurvey(int Id)
        {
            ControlSurvey controlSurvey = new ControlSurvey();
            DataSet ds = controlSurvey.fetchSurvey(Id);
            List<SurveyDatas> value = ds.Tables[0].ToList<SurveyDatas>();
            return value;
        }
        public void CreateSurveyResult(SurveyData param)
        {
            ControlSurvey controlSurvey = new ControlSurvey();
            controlSurvey.CreateSurvey(param.SurveyText, param.question11, param.question21, param.question22, param.question31, param.question41, param.question42, param.question51);
        }
        
       

    }
}
