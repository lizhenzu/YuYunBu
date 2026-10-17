using System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class MissionPointButton
    {
        public bool MissionButtonLocked { get; set; }
        public string Title { get; set; }

        public int CountData { get; set; }
        public string FunctionName { get; set; }
    }

}
