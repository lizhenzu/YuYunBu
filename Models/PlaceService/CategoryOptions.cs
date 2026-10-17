// Models/CategoryOptions.cs
namespace BulletinBoard.Models
{
    public static class CategoryOptions
    {
        public static readonly string[] All = new[] { "맛집", "카페", "레스토랑", "볼거리" };

        // 사용자가 '레스트토랑'처럼 오타를 입력해도 '레스토랑'으로 정규화
        public static string Normalize(string? src)
        {
            if (string.IsNullOrWhiteSpace(src)) return "맛집";
            var s = src.Trim();
            if (s.Contains("레스") && s.Contains("랑")) return "레스토랑";
            if (s.Contains("카페")) return "카페";
            return "맛집";
        }
    }
}
