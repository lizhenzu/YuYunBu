using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class RhapsodyOfRealityComposite
    {
        public RhapsodyOfRealitys RhapsodyOfRealitysData { get; set; }

        public List<eBibles> eBibleDatas { get; set; }

        public RhapsodyOfRealityComposite()
        {
            eBibleDatas = new List<eBibles>();
            RhapsodyOfRealitysData = new RhapsodyOfRealitys(); 
        }
    }
}
