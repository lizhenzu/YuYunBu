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
    public class ManageDevelopRGBColorGradientTable
    {

        public List<RGBColorGradientTable> getRGBColorGradientTable()
        {
            ControlDevelopRGBColorGradientTable ControlDevelopRGBColorGradientTable = new ControlDevelopRGBColorGradientTable();
            DataSet ds = ControlDevelopRGBColorGradientTable.fetchRGBColorGradientTable();
            List<RGBColorGradientTable> value = ds.Tables[0].ToList<RGBColorGradientTable>();
            return value;
        }
        public void createRGBColorGradientTable(string ColorName, string BackGroud1, string BackGroud2, string BackGroud3, string FontColor)
        {
            ControlDevelopRGBColorGradientTable ControlDevelopRGBColorGradientTable = new ControlDevelopRGBColorGradientTable();
             ControlDevelopRGBColorGradientTable.createRGBColorGradientTable(ColorName, BackGroud1, BackGroud2, BackGroud3, FontColor);
        
        }
        public void updateRGBColorGradientTable(int Id, string ColorName, string BackGroud1, string BackGroud2, string BackGroud3, string FontColor)
        {
            ControlDevelopRGBColorGradientTable ControlDevelopRGBColorGradientTable = new ControlDevelopRGBColorGradientTable();
            ControlDevelopRGBColorGradientTable.updateRGBColorGradientTable(Id, ColorName, BackGroud1, BackGroud2, BackGroud3, FontColor); 
 
        }
        public void deleteRGBColorGradientTable(int Id)
        {
            ControlDevelopRGBColorGradientTable ControlDevelopRGBColorGradientTable = new ControlDevelopRGBColorGradientTable();
            ControlDevelopRGBColorGradientTable.deleteRGBColorGradientTable(Id);
        
        }
    }
}
