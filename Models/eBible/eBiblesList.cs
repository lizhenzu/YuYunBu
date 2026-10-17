using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class eBiblesList
    {
        public List<eBibles> eBibleDatas { get; set; } 
        public eBiblesList()
        {
            eBibleDatas = new List<eBibles>();   
        }
    }
}
