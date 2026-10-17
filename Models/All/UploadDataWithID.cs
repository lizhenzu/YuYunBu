using BulletinBoard.Common;
using System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class UploadDataWithID
    {
        public List<UploadData> UploadDatas { get; set; }

        public int WeeklyWordToReadID { get; set; }
        public Category SelectedCategory { get; set; }

        public UploadDataWithID()
        {
            UploadDatas = new List<UploadData>(); 
        }
    }
}
