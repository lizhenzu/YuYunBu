namespace BulletinBoard.DataAccessLayer.Birthday.Models
{
    public class BirthdayUpdateDto
    {
        public int PersonId { get; set; }
        public int? bYear { get; set; }
        public int? bMonth { get; set; }
        public int? bDay { get; set; }
        public string? plusMins { get; set; }
        public string publishYN { get; set; } = "Y";
    }
}
