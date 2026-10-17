using System;
using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Worldview
{ 

    public class LessonSubmitResult
    {
        public bool Ok { get; set; }
        public bool AlreadySubmitted { get; set; }
        public bool IsAllCorrect { get; set; }
        public int BasePoint { get; set; }
        public int BonusPoint { get; set; }
        public int TotalPoint { get; set; }
        public string Message { get; set; } = "";
    }
}
