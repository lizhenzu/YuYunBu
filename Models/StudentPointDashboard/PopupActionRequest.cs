using BulletinBoard.Enums;

namespace BulletinBoard.ViewModels.Dashboard
{
    public class PopupActionRequest
    {
        public int Milestone { get; set; }
        public PopupActionType ActionType { get; set; }
    }
}
