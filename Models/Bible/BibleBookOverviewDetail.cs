using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    using System.ComponentModel.DataAnnotations;

    public class BibleBookOverviewDetail
    {

        [Display(Name = "No.")]
        public long sequenceNo { get; set; }

        [Key]
        public int Id { get; set; }

        //[Required]
        [MaxLength(50)]
        public string BookName { get; set; }

        [MaxLength(200)]
        public string Author { get; set; }

        public string AuthorDescription { get; set; }

        [MaxLength(20)]
        public string WrittenDateFrom { get; set; }

        [MaxLength(20)]
        public string WrittenDateTo { get; set; }

        [MaxLength(100)]
        public string AlternativeDate { get; set; }

        [MaxLength(200)]
        public string WrittenPlace { get; set; }

        public string WrittenTarget { get; set; }

        [MaxLength(100)]
        public string HistoricalPeriod { get; set; }

        public string HistoricalBackground { get; set; }

        [MaxLength(300)]
        public string Keywords { get; set; }

        public string ContentSummary { get; set; }
        public string SummaryURL1 { get; set; }
        public string SummaryURL2 { get; set; }
    }

}