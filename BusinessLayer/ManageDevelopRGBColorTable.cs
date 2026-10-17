using BulletinBoard.DataAccessLayer;
using BulletinBoard.Models;
using BulletinBoard.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace BulletinBoard.BusinessLayer
{
    public class ManageDevelopRGBColorTable
    {

        public List<RGBColorTable> getRGBColorTable()
        {
            ControlDevelopRGBColorTable ControlDevelopRGBColorTable = new ControlDevelopRGBColorTable();
            DataSet ds = ControlDevelopRGBColorTable.fetchRGBColorTable();
            List<RGBColorTable> value = ds.Tables[0].ToList<RGBColorTable>();
            return value;
        }
        public void createRGBColorTable(string ColorName, string BackGroud, string FontColor,string ColorType)
        {
            ControlDevelopRGBColorTable ControlDevelopRGBColorTable = new ControlDevelopRGBColorTable();
             ControlDevelopRGBColorTable.createRGBColorTable(ColorName, BackGroud, FontColor, ColorType);
        
        }
        public void updateRGBColorTable(int Id, string ColorName, string BackGroud, string FontColor, string ColorType)
        {
            ControlDevelopRGBColorTable ControlDevelopRGBColorTable = new ControlDevelopRGBColorTable();
            ControlDevelopRGBColorTable.updateRGBColorTable(Id, ColorName, BackGroud, FontColor, ColorType); 
 
        }
        public void deleteRGBColorTable(int Id)
        {
            ControlDevelopRGBColorTable ControlDevelopRGBColorTable = new ControlDevelopRGBColorTable();
            ControlDevelopRGBColorTable.deleteRGBColorTable(Id);
        
        }
    }
}
