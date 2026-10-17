using System;
using System.ComponentModel.DataAnnotations;

namespace BulletinBoard.Models
{
    public class ImageUploadModel
    {
        public string UserId { get; set; }
        public string ProfilePicture { get; set; }
    }

}
