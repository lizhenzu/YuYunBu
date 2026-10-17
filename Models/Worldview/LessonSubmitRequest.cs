using System;
using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Worldview
{
    public class LessonSubmitRequest
    {
        public int LessonNo { get; set; }
        public Dictionary<string, string[]>? Answers { get; set; }
    }
     
}
