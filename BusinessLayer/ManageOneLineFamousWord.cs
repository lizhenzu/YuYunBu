using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;

namespace BulletinBoard.BusinessLayer
{
    public class ManageOneLineFamousWord
    {
        public List<OneLineFamousWords> getOneLineFamousWord(int Id)
        {
            ControlOneLineFamousWord controlOneLineFamousWord = new ControlOneLineFamousWord();
            DataSet ds = controlOneLineFamousWord.fetchOneLineFamousWord(Id);
            List<OneLineFamousWords> value = ds.Tables[0].ToList<OneLineFamousWords>();
            return value;
        }
        public void CreateOneLineFamousWord(OneLineFamousWords param)
        {
            ControlOneLineFamousWord controlOneLineFamousWord = new ControlOneLineFamousWord();
            controlOneLineFamousWord.CreateOneLineFamousWord(param.OneLineText);
        }
        public void UpdateOneLineFamousWord(OneLineFamousWords param)
        {
            ControlOneLineFamousWord controlOneLineFamousWord = new ControlOneLineFamousWord();
            controlOneLineFamousWord.UpdateOneLineFamousWord(param.Id, param.OneLineText);
        }


        public void DeleteOneLineFamousWord(int Id)
        {
            ControlOneLineFamousWord controlOneLineFamousWord = new ControlOneLineFamousWord();
            controlOneLineFamousWord.DeleteOneLineFamousWord(Id);
        }

    }
}
