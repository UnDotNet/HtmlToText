
namespace UnDotNet.HtmlToText.Tests;

public partial class TagsTests
{
    [TestClass]
    public class BlockquoteTests
    {
        
        private string htmlToText(string? html, HtmlToTextOptions? options = null, Dictionary<string, string>? metadata = null) =>
            new HtmlToTextConverter().Convert(html, options, metadata);

        [TestMethod]
        public void ShouldHandleFormatSingleLineBlockquote()
        {
            string html = "foo<blockquote>test</blockquote>bar";
            string expected = "foo\n\n> test\n\nbar";
            htmlToText(html, null).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldFormatMultiLineBlockquote()
        {
            string html = "<blockquote>a<br/>b</blockquote>";
            string expected = "> a\n> b";
            htmlToText(html, null).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldTrimNewlinesUnlessDisabled()
        {
            string html = "<blockquote><br/>a<br/><br/><br/></blockquote>";
            string expectedDefault = "> a";
            htmlToText(html, null).ShouldBe(expectedDefault);
            var options = new HtmlToTextOptions();
            options.BlockQuote.options.trimEmptyLines = false;
            string expectedCustom = "> \n> a\n> \n> \n> ";
            htmlToText(html, options).ShouldBe(expectedCustom);
        }
    }
}