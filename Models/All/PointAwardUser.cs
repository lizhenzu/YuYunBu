namespace BulletinBoard.Models
{
    public class PointAwardUser  : PointByUserName
    {
        public int? ProductID { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string PointRequirement { get; set; }
        public int? UnitPrice { get; set; }
    }
}
 