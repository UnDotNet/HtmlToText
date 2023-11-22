
namespace UnDotNet.HtmlToText.Tests;

public partial class TagsTests
{
    [TestClass]
    public class PTests
    {
        private string htmlToText(string? html, HtmlToTextOptions? options = null, Dictionary<string, string>? metadata = null) =>
            new HtmlToTextConverter().Convert(html, options, metadata);

        [TestMethod]
        public void ShouldSeparateParagraphsFromSurroundingContentByTwoLinebreaks()
        {
            string html = "text<p>first</p><p>second</p>text";
            string expected = "text\n\nfirst\n\nsecond\n\ntext";
            htmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldAllowToChangeTheNumberOfLinebreaks()
        {
            string html = "text<p>first</p><p>second</p>text";
            var options = new HtmlToTextOptions();
            options.P.options.leadingLineBreaks = 1;
            options.P.options.trailingLineBreaks = 1;
            string expected = "text\nfirst\nsecond\ntext";
            htmlToText(html, options).ShouldBe(expected);
        }
    }
}