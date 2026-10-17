using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Html;

namespace BulletinBoard.Helpers
{
    /// <summary>
    /// 기도문 본문을 화면에 뿌릴 때 쓰는 도우미.
    /// DB 에는 이름 자리가 **○○(이)** 처럼 별표로 감싸여 저장되어 있는데,
    /// 그대로 출력하면 별표가 그냥 보이므로 강조 표시로 바꿔 준다.
    /// </summary>
    public static class PrayTextHelper
    {
        // **○○(이)** / ***○○(이)*** 둘 다 받는다
        private static readonly Regex MarkPattern =
            new Regex(@"\*{2,3}(.+?)\*{2,3}", RegexOptions.Compiled | RegexOptions.Singleline);

        /// <summary>
        /// 본문을 HTML 로 이스케이프한 뒤, 별표로 감싼 부분만 &lt;span class="nm"&gt; 으로 감싼다.
        /// 줄바꿈은 건드리지 않으므로 white-space:pre-line 을 그대로 쓸 수 있다.
        /// </summary>
        public static IHtmlContent MarkNames(string? raw)
        {
            if (string.IsNullOrEmpty(raw)) return HtmlString.Empty;

            var encoded = System.Net.WebUtility.HtmlEncode(raw);
            var marked = MarkPattern.Replace(encoded, "<span class=\"nm\">$1</span>");

            // 짝이 맞지 않아 남은 별표는 지운다
            marked = marked.Replace("**", string.Empty);

            return new HtmlString(marked);
        }

        // 페이지마다 CSS 가 제각각인 화면에서 쓴다.
        // 클래스가 없어도 같은 모양(꿀빛 밑줄)이 나오도록 style 을 직접 넣는다.
        private const string InlineStyle =
            "color:#B8791E;font-weight:700;" +
            "background:linear-gradient(180deg,transparent 62%,#FBE3B8 62%);" +
            "padding:0 2px;border-radius:3px;";

        /// <summary>
        /// MarkNames 와 같으나, 스타일을 인라인으로 붙여 어느 화면에서든 동일하게 보이게 한다.
        /// </summary>
        public static IHtmlContent MarkNamesInline(string? raw)
        {
            if (string.IsNullOrEmpty(raw)) return HtmlString.Empty;

            var encoded = System.Net.WebUtility.HtmlEncode(raw);
            var marked = MarkPattern.Replace(encoded,
                "<span class=\"nm\" style=\"" + InlineStyle + "\">$1</span>");

            marked = marked.Replace("**", string.Empty);

            return new HtmlString(marked);
        }

        /// <summary>
        /// 자바스크립트에서 쓰기 위한 CSS 조각.
        /// </summary>
        public static string NameSpanStyle => InlineStyle;
    }
}
