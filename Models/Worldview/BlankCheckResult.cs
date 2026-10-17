using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Worldview
{

    public class BlankCheckResult
    {
        public bool IsAllCorrect { get; set; }
        public Dictionary<string, bool> PerGroupCorrect { get; set; } = new();
        public string Message { get; set; } = "";
    }
}
