using System.Collections.Generic;

namespace BulletinBoard.ViewModels.Worldview
{
    // 예: ViewModels/Worldview/BlankCharMaps.cs 또는 Services/WorldView/Models 폴더

    public class BlankCharMaps
    {
        // groupKey -> (CharIndex -> AnswerChar)
        public Dictionary<string, Dictionary<int, string>> Answer { get; }
            = new(StringComparer.OrdinalIgnoreCase);

        // groupKey -> (CharIndex -> InitialChar)
        public Dictionary<string, Dictionary<int, string>> Initial { get; }
            = new(StringComparer.OrdinalIgnoreCase);
    }

}
