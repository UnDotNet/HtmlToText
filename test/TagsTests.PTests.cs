
namespace UnDotNet.HtmlToText.Tests;

public partial class TagsTests
{
    [TestClass]
    public class PTests
    {
        private static string HtmlToText(string? html, HtmlToTextOptions? options = null, Dictionary<string, string>? metadata = null) =>
            new HtmlToTextConverter().Convert(html, options, metadata);

        [TestMethod]
        public void ShouldSeparateParagraphsFromSurroundingContentByTwoLinebreaks()
        {
            var html = "text<p>first</p><p>second</p>text";
            var expected = "text\n\nfirst\n\nsecond\n\ntext";
            HtmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldAllowToChangeTheNumberOfLinebreaks()
        {
            var html = "text<p>first</p><p>second</p>text";
            var options = new HtmlToTextOptions();
            options.P.Options.LeadingLineBreaks = 1;
            options.P.Options.TrailingLineBreaks = 1;
            var expected = "text\nfirst\nsecond\ntext";
            HtmlToText(html, options).ShouldBe(expected);
        }
    }
}