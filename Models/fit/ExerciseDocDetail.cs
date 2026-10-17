// Models/Fitness/ExerciseDocDetail.cs
using System.Collections.Generic;

using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class ExerciseDocDetail
    {
        public int ExerciseDocId { get; set; }
        public string Slug { get; set; }
        public string TitleKo { get; set; }
        public string TitleEn { get; set; }
        public string Subtitle { get; set; }
        public string Audience { get; set; }
        public string LevelText { get; set; }
        public string Keywords { get; set; }
        public string Description { get; set; }

        public SeoMetaDto Seo { get; set; }              // null 가능
        public List<ChipDto> Chips { get; set; }
        public List<IllustrationDto> Illustrations { get; set; }
        public List<SectionDto> Sections { get; set; }
        public List<NavigationLinkDto> Navigation { get; set; }

        public ExerciseDocDetail()
        {
            Slug = "";
            TitleKo = "";
            TitleEn = "";
            Subtitle = "";
            Audience = "";
            LevelText = "";
            Keywords = "";
            Description = "";
            Chips = new List<ChipDto>();
            Illustrations = new List<IllustrationDto>();
            Sections = new List<SectionDto>();
            Navigation = new List<NavigationLinkDto>();
        }
    }

    public class SeoMetaDto
    {
        public string HtmlTitle { get; set; }
        public string MetaDescription { get; set; }

        public SeoMetaDto()
        {
            HtmlTitle = "";
            MetaDescription = "";
        }
    }

    public class ChipDto
    {
        public int ChipId { get; set; }
        public string ChipText { get; set; }
        public int SortOrder { get; set; }

        public ChipDto()
        {
            ChipText = "";
        }
    }

    public class IllustrationDto
    {
        public int IllustrationId { get; set; }
        public string Url { get; set; }
        public string AltText { get; set; }
        public int SortOrder { get; set; }

        public IllustrationDto()
        {
            Url = "";
            AltText = "";
        }
    }

    public class SectionDto
    {
        public int SectionId { get; set; }
        public string SectionType { get; set; } // intro/howto/...
        public string Eyebrow { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Body { get; set; }
        public int SortOrder { get; set; }

        public List<SectionListItemDto> Items { get; set; }
        public PrescriptionDto Prescription { get; set; } // null 가능

        public SectionDto()
        {
            SectionType = "";
            Eyebrow = "";
            Title = "";
            Subtitle = "";
            Body = "";
            Items = new List<SectionListItemDto>();
        }
    }

    public class SectionListItemDto
    {
        public int ListItemId { get; set; }
        public string ItemType { get; set; } // ol/ul/paragraph
        public string Content { get; set; }
        public int SortOrder { get; set; }

        public SectionListItemDto()
        {
            ItemType = "";
            Content = "";
        }
    }

    public class PrescriptionDto
    {
        public int PrescriptionId { get; set; }
        public string Note { get; set; }
        public List<PrescriptionRowDto> Rows { get; set; }

        public PrescriptionDto()
        {
            Note = "";
            Rows = new List<PrescriptionRowDto>();
        }
    }

    public class PrescriptionRowDto
    {
        public int PrescriptionRowId { get; set; }
        public string LabelText { get; set; }
        public string ValueText { get; set; }
        public int SortOrder { get; set; }

        public PrescriptionRowDto()
        {
            LabelText = "";
            ValueText = "";
        }
    }

    public class NavigationLinkDto
    {
        public int NavLinkId { get; set; }
        public string Label { get; set; }
        public string Href { get; set; }
        public string StyleHint { get; set; }
        public int SortOrder { get; set; }

        public NavigationLinkDto()
        {
            Label = "";
            Href = "";
            StyleHint = "";
        }
    }
}
