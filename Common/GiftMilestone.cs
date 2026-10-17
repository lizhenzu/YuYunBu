namespace BulletinBoard.Models.Gift
{
    public static class GiftMilestone
    {
        public const int Step = 100;
        public const int Min = 100;
        public const int Max = 2000;

        public static int GetCurrentMilestone(int integratedTotal)
        {
            if (integratedTotal < Min) return 0;
            var ms = (integratedTotal / Step) * Step;
            if (ms > Max) ms = Max;
            return ms;
        }
    }
}
