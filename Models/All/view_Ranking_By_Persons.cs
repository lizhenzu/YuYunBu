using System;
using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class view_Ranking_By_Persons : view_Ranking_By_Person
    {
        public long sequenceNo { get; set; }
    }
}
 