
namespace UnDotNet.HtmlToText.Tests;

public partial class TagsTests
{
    [TestClass]
    public class BlockquoteTests
    {
        
        private static string HtmlToText(string? html, HtmlToTextOptions? options = null, Dictionary<string, string>? metadata = null) =>
            new HtmlToTextConverter().Convert(html, options, metadata);

        [TestMethod]
        public void ShouldHandleFormatSingleLineBlockquote()
        {
            var html = "foo<blockquote>test</blockquote>bar";
            var expected = "foo\n\n> test\n\nbar";
            HtmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldFormatMultiLineBlockquote()
        {
            var html = "<blockquote>a<br/>b</blockquote>";
            var expected = "> a\n> b";
            HtmlToText(html).ShouldBe(expected);
        }

        [TestMethod]
        public void ShouldTrimNewlinesUnlessDisabled()
        {
            var html = "<blockquote><br/>a<br/><br/><br/></blockquote>";
            var expectedDefault = "> a";
            HtmlToText(html).ShouldBe(expectedDefault);
            var options = new HtmlToTextOptions();
            options.BlockQuote.Options.TrimEmptyLines = false;
            var expectedCustom = "> \n> a\n> \n> \n> ";
            HtmlToText(html, options).ShouldBe(expectedCustom);
        }
    }
}